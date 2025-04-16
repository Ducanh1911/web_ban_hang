using System;
using System.Web;
using System.Web.Mvc;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Web.Script.Serialization;

namespace BaiTap.VNPay
{
    public class VNPayController : Controller
    {
        private readonly VNPayService _vnPayService;

        public VNPayController()
        {
            _vnPayService = new VNPayService();
        }

        [HttpPost]
        public JsonResult CreatePayment()
        {
            try
            {
                string requestBody;
                using (var reader = new System.IO.StreamReader(Request.InputStream))
                {
                    requestBody = reader.ReadToEnd();
                }

                var serializer = new JavaScriptSerializer();
                var request = serializer.Deserialize<PaymentRequest>(requestBody);

                if (request == null)
                {
                    return Json(new { success = false, error = "Dữ liệu không hợp lệ" }, JsonRequestBehavior.AllowGet);
                }

                if (request.Amount <= 0)
                {
                    return Json(new { success = false, error = "Số tiền không hợp lệ" }, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrEmpty(request.OrderId))
                {
                    return Json(new { success = false, error = "Mã đơn hàng không hợp lệ" }, JsonRequestBehavior.AllowGet);
                }

                var paymentUrl = _vnPayService.CreatePaymentUrl(
                    request.Amount,
                    request.OrderInfo,
                    request.OrderId
                );

                return Json(new { success = true, paymentUrl = paymentUrl }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Có lỗi xảy ra: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult PaymentCallback()
        {
            try
            {
                var response = new PaymentResponse
                {
                    OrderId = Request.QueryString["vnp_TxnRef"],
                    TransactionId = Request.QueryString["vnp_TransactionNo"],
                    ResponseCode = Request.QueryString["vnp_ResponseCode"],
                    Amount = decimal.Parse(Request.QueryString["vnp_Amount"]) / 100,
                    OrderInfo = Request.QueryString["vnp_OrderInfo"],
                    PaymentDate = DateTime.ParseExact(Request.QueryString["vnp_PayDate"], "yyyyMMddHHmmss", null),
                    IsValid = _vnPayService.ValidateResponse(Request.QueryString)
                };

                return View("PaymentResult", response);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xử lý kết quả thanh toán: " + ex.Message;
                return RedirectToAction("Index", "Order", new { area = "Customer" });
            }
        }
    }

    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public string OrderInfo { get; set; }
        public string OrderId { get; set; }
    }

    public class PaymentResponse
    {
        public string OrderId { get; set; }
        public string TransactionId { get; set; }
        public string ResponseCode { get; set; }
        public decimal Amount { get; set; }
        public string OrderInfo { get; set; }
        public DateTime PaymentDate { get; set; }
        public bool IsValid { get; set; }
    }
}