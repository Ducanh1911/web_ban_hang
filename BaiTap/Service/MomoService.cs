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
                _ipnUrl = ConfigurationManager.AppSettings["MomoIpnUrl"];
                _returnUrl = ConfigurationManager.AppSettings["MomoReturnUrl"];

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

        private string GenerateUniqueId()
        {
            return DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid().ToString("N").Substring(0, 8);
        }

        public async Task<string> CreatePaymentRequest(string orderId, decimal amount, string orderInfo)
        {
            try
            {
                // Tạo requestId và orderId duy nhất
                var requestId = GenerateUniqueId();
                var uniqueOrderId = $"{orderId}_{requestId}";
                
                // Chuyển đổi amount thành số nguyên
                var amountInt = (long)Math.Round(amount);
                
                // Tạo dictionary chứa các tham số theo thứ tự alphabet
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

                // Tạo chuỗi raw signature từ dictionary đã sắp xếp
                var rawSignature = CreateRawSignature(parameters);
                Debug.WriteLine($"Raw Signature: {rawSignature}");

                var signature = ComputeHmacSha256(rawSignature, _secretKey);
                Debug.WriteLine($"Generated Signature: {signature}");

                // Tạo request body với orderId duy nhất
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
                Debug.WriteLine($"Request Body: {jsonRequest}");

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                    
                    Debug.WriteLine($"Sending request to: {_endpoint}");
                    var response = await client.PostAsync(_endpoint, content);
                    
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Response Status: {response.StatusCode}");
                    Debug.WriteLine($"Response Content: {responseContent}");

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

                        Debug.WriteLine($"Result Code: {resultCode}");
                        Debug.WriteLine($"Message: {message}");
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

        public bool ValidateSignature(string rawHash, string signature)
        {
            try
            {
                var calculatedSignature = ComputeHmacSha256(rawHash, _secretKey);
                Debug.WriteLine($"Raw Hash: {rawHash}");
                Debug.WriteLine($"Received Signature: {signature}");
                Debug.WriteLine($"Calculated Signature: {calculatedSignature}");
                return calculatedSignature.Equals(signature, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error validating signature: {ex.Message}");
                return false;
            }
        }
    }
}