using BaiTap.Models;
using BaiTap.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BaiTap.Areas.Customer.Controllers
{
    public class PaymentController : Controller
    {

        private readonly ShopEntities _db;
        private readonly MomoService _momoService;

        public PaymentController(ShopEntities db)
        {
            _db = db;
            _momoService = new MomoService();
        }
        // GET: Customer/Payment
        [HttpPost]
        public async Task<ActionResult> PayWithMomo(int orderId)
        {
            try
            {
                var order = _db.Orders.Find(orderId);
                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                if (order.status == "Paid")
                {
                    return Json(new { success = false, message = "Đơn hàng đã được thanh toán" });
                }

                Debug.WriteLine($"Creating payment request for order {orderId} with amount {order.finalAmount}");

                var paymentUrl = await _momoService.CreatePaymentRequest(
                    orderId.ToString(),
                    order.finalAmount,
                    $"Thanh toán đơn hàng #{orderId}"
                );

                Debug.WriteLine($"Payment URL received: {paymentUrl}");

                return Json(new { success = true, paymentUrl });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in PayWithMomo: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult MomoReturn(
            string partnerCode,
            string orderId,
            string requestId,
            string amount,
            string orderInfo,
            string orderType,
            string transId,
            string resultCode,
            string message,
            string payType,
            string responseTime,
            string extraData,
            string signature)
        {
            try
            {
                Debug.WriteLine($"Received MOMO return callback for order {orderId}");

                // Validate signature
                var rawHash = $"accessKey={_momoService.AccessKey}&amount={amount}&extraData={extraData}&message={message}&orderId={orderId}&orderInfo={orderInfo}&orderType={orderType}&partnerCode={partnerCode}&payType={payType}&requestId={requestId}&responseTime={responseTime}&resultCode={resultCode}&transId={transId}";

                if (!_momoService.ValidateSignature(rawHash, signature))
                {
                    Debug.WriteLine("Invalid signature received");
                    TempData["ErrorMessage"] = "Chữ ký không hợp lệ";
                    return RedirectToAction("Index");
                }

                if (resultCode == "0")
                {
                    var order = _db.Orders.Find(int.Parse(orderId));
                    if (order != null)
                    {
                        order.status = "Paid";
                        _db.SaveChanges();
                        Debug.WriteLine($"Order {orderId} marked as paid");
                    }
                    TempData["SuccessMessage"] = "Thanh toán thành công!";
                }
                else
                {
                    Debug.WriteLine($"Payment failed with message: {message}");
                    TempData["ErrorMessage"] = $"Thanh toán thất bại: {message}";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in MomoReturn: {ex.Message}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xử lý thanh toán";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult MomoIpn(
            string partnerCode,
            string orderId,
            string requestId,
            string amount,
            string orderInfo,
            string orderType,
            string transId,
            string resultCode,
            string message,
            string payType,
            string responseTime,
            string extraData,
            string signature)
        {
            try
            {
                Debug.WriteLine($"Received MOMO IPN callback for order {orderId}");

                // Validate signature
                var rawHash = $"accessKey={_momoService.AccessKey}&amount={amount}&extraData={extraData}&message={message}&orderId={orderId}&orderInfo={orderInfo}&orderType={orderType}&partnerCode={partnerCode}&payType={payType}&requestId={requestId}&responseTime={responseTime}&resultCode={resultCode}&transId={transId}";

                if (!_momoService.ValidateSignature(rawHash, signature))
                {
                    Debug.WriteLine("Invalid signature received in IPN");
                    return Json(new { message = "Invalid signature" });
                }

                if (resultCode == "0")
                {
                    var order = _db.Orders.Find(int.Parse(orderId));
                    if (order != null)
                    {
                        order.status = "Processing";
                        _db.SaveChanges();
                        Debug.WriteLine($"Order {orderId} marked as paid via IPN");
                    }
                }
                else
                {
                    Debug.WriteLine($"Payment failed in IPN with message: {message}");
                }

                return Json(new { message = "Success" });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in MomoIpn: {ex.Message}");
                return Json(new { message = "Error processing IPN" });
            }
        }
    }
}
