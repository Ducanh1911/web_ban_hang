using System;
using System.Configuration;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace BaiTap.Service
{
    public class MomoService
    {
        private readonly string _partnerCode;
        private readonly string _accessKey;
        private readonly string _secretKey;
        private readonly string _endpoint;
        private readonly string _ipnUrl;
        private readonly string _returnUrl;
        public string AccessKey => _accessKey;

        public MomoService()
        {
            try
            {
                _partnerCode = ConfigurationManager.AppSettings["MomoPartnerCode"];
                _accessKey = ConfigurationManager.AppSettings["MomoAccessKey"];
                _secretKey = ConfigurationManager.AppSettings["MomoSecretKey"];
                _endpoint = ConfigurationManager.AppSettings["MomoEndpoint"];
                _ipnUrl = ConfigurationManager.AppSettings["NotifyUrl"];
                _returnUrl = ConfigurationManager.AppSettings["ReturnUrl"];

                Debug.WriteLine($"MomoService Configuration - partnerCode: {_partnerCode}, accessKey: {_accessKey}, endpoint: {_endpoint}, returnUrl: {_returnUrl}, ipnUrl: {_ipnUrl}");

                if (string.IsNullOrEmpty(_partnerCode) || string.IsNullOrEmpty(_accessKey) ||
                    string.IsNullOrEmpty(_secretKey) || string.IsNullOrEmpty(_endpoint) ||
                    string.IsNullOrEmpty(_ipnUrl) || string.IsNullOrEmpty(_returnUrl))
                {
                    throw new Exception("MOMO configuration is missing or invalid");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing MomoService: {ex.Message}");
                throw;
            }
        }

        private string CreateRawSignature(Dictionary<string, string> parameters)
        {
            var sortedParams = parameters.OrderBy(x => x.Key)
                                       .Select(x => $"{x.Key}={x.Value}");
            return string.Join("&", sortedParams);
        }

        private string CreateRawSignatureForQuery(Dictionary<string, string> parameters)
        {
            // MOMO requires the signature for QueryTransaction in this order: accessKey, orderId, partnerCode, requestId
            return $"accessKey={parameters["accessKey"]}&orderId={parameters["orderId"]}&partnerCode={parameters["partnerCode"]}&requestId={parameters["requestId"]}";
        }

        public async Task<string> CreatePaymentRequest(string orderId, decimal amount, string orderInfo)
        {
            try
            {
                // Generate a unique requestId
                var requestId = Guid.NewGuid().ToString();
                var uniqueOrderId = orderId;

                // Convert amount to integer (MOMO requires amount in VND as an integer)
                var amountInt = (long)Math.Round(amount);

                // Create parameters for signature
                var parameters = new Dictionary<string, string>
                {
                    {"accessKey", _accessKey},
                    {"amount", amountInt.ToString()},
                    {"extraData", ""},
                    {"ipnUrl", _ipnUrl},
                    {"orderId", uniqueOrderId},
                    {"orderInfo", orderInfo},
                    {"partnerCode", _partnerCode},
                    {"redirectUrl", _returnUrl},
                    {"requestId", requestId},
                    {"requestType", "captureWallet"}
                };

                // Generate raw signature string
                var rawSignature = CreateRawSignature(parameters);
                Debug.WriteLine($"Raw Signature (CreatePaymentRequest): {rawSignature}");

                // Compute HMAC-SHA256 signature
                var signature = ComputeHmacSha256(rawSignature, _secretKey);
                Debug.WriteLine($"Generated Signature (CreatePaymentRequest): {signature}");

                // Create request body
                var requestBody = new
                {
                    partnerCode = _partnerCode,
                    partnerName = "Test",
                    storeId = "MomoTestStore",
                    requestId = requestId,
                    amount = amountInt,
                    orderId = uniqueOrderId,
                    orderInfo = orderInfo,
                    redirectUrl = _returnUrl,
                    ipnUrl = _ipnUrl,
                    lang = "vi",
                    requestType = "captureWallet",
                    autoCapture = true,
                    extraData = "",
                    signature = signature
                };

                var jsonRequest = JsonConvert.SerializeObject(requestBody);
                Debug.WriteLine($"Request Body (CreatePaymentRequest): {jsonRequest}");

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                    Debug.WriteLine($"Sending request to: {_endpoint}");
                    var response = await client.PostAsync(_endpoint, content);

                    var responseContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Response Status (CreatePaymentRequest): {response.StatusCode}");
                    Debug.WriteLine($"Response Content (CreatePaymentRequest): {responseContent}");

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"HTTP request failed with status code: {response.StatusCode}, Content: {responseContent}");
                    }

                    if (string.IsNullOrEmpty(responseContent))
                    {
                        throw new Exception("Empty response received from MOMO");
                    }

                    try
                    {
                        var responseObject = JObject.Parse(responseContent);

                        var resultCode = responseObject["resultCode"]?.ToString();
                        var payUrl = responseObject["payUrl"]?.ToString();
                        var message = responseObject["message"]?.ToString() ?? "Unknown error";

                        Debug.WriteLine($"Result Code (CreatePaymentRequest): {resultCode}");
                        Debug.WriteLine($"Message (CreatePaymentRequest): {message}");
                        Debug.WriteLine($"Pay URL: {payUrl}");

                        if (string.IsNullOrEmpty(resultCode))
                        {
                            throw new Exception($"Invalid response format: missing resultCode. Response: {responseContent}");
                        }

                        if (resultCode == "0" && !string.IsNullOrEmpty(payUrl))
                        {
                            return payUrl;
                        }

                        throw new Exception($"MOMO Error: {message}. Result Code: {resultCode}");
                    }
                    catch (JsonReaderException ex)
                    {
                        throw new Exception($"Invalid JSON response: {ex.Message}. Response content: {responseContent}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating payment request: {ex.Message}");
                throw;
            }
        }

        public async Task<(bool Success, string Message)> QueryTransaction(string orderId, string requestId)
        {
            try
            {
                var parameters = new Dictionary<string, string>
                {
                    { "accessKey", _accessKey },
                    { "partnerCode", _partnerCode },
                    { "requestId", requestId },
                    { "orderId", orderId }
                };

                // Generate raw signature string in the correct order
                var rawSignature = CreateRawSignatureForQuery(parameters);
                Debug.WriteLine($"Raw Signature (QueryTransaction): {rawSignature}");

                var signature = ComputeHmacSha256(rawSignature, _secretKey);
                Debug.WriteLine($"Generated Signature (QueryTransaction): {signature}");

                var requestBody = new
                {
                    partnerCode = _partnerCode,
                    requestId = requestId,
                    orderId = orderId,
                    lang = "vi",
                    signature = signature
                };

                var jsonRequest = JsonConvert.SerializeObject(requestBody);
                Debug.WriteLine($"QueryTransaction Request Body: {jsonRequest}");

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                    var queryEndpoint = "https://test-payment.momo.vn/v2/gateway/api/query";
                    Debug.WriteLine($"Sending QueryTransaction request to: {queryEndpoint}");
                    var response = await client.PostAsync(queryEndpoint, content);

                    var responseContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"QueryTransaction Response Status: {response.StatusCode}");
                    Debug.WriteLine($"QueryTransaction Response Content: {responseContent}");

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"QueryTransaction failed with status code: {response.StatusCode}, Content: {responseContent}");
                    }

                    var responseObject = JObject.Parse(responseContent);
                    var resultCode = responseObject["resultCode"]?.ToString();
                    var message = responseObject["message"]?.ToString() ?? "Unknown error";

                    Debug.WriteLine($"QueryTransaction Result Code: {resultCode}, Message: {message}");

                    if (resultCode == "0")
                    {
                        var transId = responseObject["transId"]?.ToString();
                        if (!string.IsNullOrEmpty(transId))
                        {
                            return (true, "Thanh toán thành công");
                        }
                        else
                        {
                            return (false, "Giao dịch chưa hoàn tất");
                        }
                    }

                    return (false, $"MOMO Error: {message}. Result Code: {resultCode}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in QueryTransaction: {ex.Message}");
                return (false, ex.Message);
            }
        }

        private string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(messageBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}