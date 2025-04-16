using System;
using System.Collections.Generic;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Specialized;
using System.Linq;

namespace BaiTap.VNPay
{
    public class VNPayService
    {
        public string CreatePaymentUrl(decimal amount, string orderInfo, string orderId)
        {
            string returnUrl = VNPayConfig.ReturnUrl;
            string baseUrl = VNPayConfig.BaseUrl;

            var vnpayData = new SortedList<string, string>();

            vnpayData.Add("vnp_Version", VNPayConfig.Version);
            vnpayData.Add("vnp_Command", VNPayConfig.Command);
            vnpayData.Add("vnp_TmnCode", VNPayConfig.TmnCode);
            vnpayData.Add("vnp_Amount", ((long)(amount * 100)).ToString());
            vnpayData.Add("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpayData.Add("vnp_CurrCode", VNPayConfig.CurrCode);
            vnpayData.Add("vnp_IpAddr", HttpContext.Current.Request.UserHostAddress);
            vnpayData.Add("vnp_Locale", VNPayConfig.Locale);
            vnpayData.Add("vnp_OrderInfo", orderInfo);
            vnpayData.Add("vnp_OrderType", "other");
            vnpayData.Add("vnp_ReturnUrl", returnUrl);
            vnpayData.Add("vnp_TxnRef", orderId);

            // Tạo chuỗi rawData để ký
            var data = new StringBuilder();
            foreach (var item in vnpayData)
            {
                data.Append($"{item.Key}={HttpUtility.UrlEncode(item.Value)}&");
            }

            string rawData = data.ToString().TrimEnd('&');

            // Tạo hash
            string secureHash = HmacSHA512(VNPayConfig.HashSecret, rawData);
            vnpayData.Add("vnp_SecureHash", secureHash);

            // Tạo URL thanh toán
            var paymentUrl = new StringBuilder(baseUrl + "?");
            foreach (var item in vnpayData)
            {
                paymentUrl.Append($"{item.Key}={HttpUtility.UrlEncode(item.Value)}&");
            }
            //Console.WriteLine("✅ RawData to hash: " + rawData);
            //Console.WriteLine("✅ SecureHash: " + secureHash);
            //Console.WriteLine("✅ Full payment URL: " + paymentUrl);

            return paymentUrl.ToString().TrimEnd('&');

        }

        public bool ValidateResponse(NameValueCollection collection)
        {
            var vnpData = new SortedList<string, string>();
            foreach (string key in collection.AllKeys)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_") && key != "vnp_SecureHash" && key != "vnp_SecureHashType")
                {
                    vnpData.Add(key, collection[key]);
                }
            }

            string rawData = string.Join("&", vnpData.Select(x => $"{x.Key}={HttpUtility.UrlEncode(x.Value)}"));
            string secureHash = HmacSHA512(VNPayConfig.HashSecret, rawData);
            string vnpSecureHash = collection["vnp_SecureHash"];

            return secureHash.Equals(vnpSecureHash, StringComparison.InvariantCultureIgnoreCase);
        }


        private string HmacSHA512(string key, string inputData)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
