using System;

namespace BaiTap.VNPay
{
    public class VNPayConfig
    {
        public static string Version = "2.1.0";
        public static string TmnCode = "JYDV333L"; // Mã website tại VNPAY 
        public static string HashSecret = "U9O8LKK66INW3JFSZJ330EN6CRD54W8W"; 
        public static string BaseUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        public static string Command = "pay";
        public static string CurrCode = "VND";
        public static string Locale = "vn";
        public static string ReturnUrl = "http://localhost:44378/VNPay/PaymentCallback"; // Thay đổi port theo ứng dụng của bạn
    }
}