using BaiTap.App_Start;
using BaiTap.Models;
using BaiTap.Service;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Diagnostics;

namespace BaiTap.Areas.Customer.Controllers
{
    [RoleUser]
    public class OrderController : Controller
    {
        private readonly ShopEntities _db;
        private readonly MomoService _momoService;

        public OrderController(ShopEntities db)
        {
            _db = db;
            _momoService = new MomoService();
        }

        // GET: Customer/Order
        public ActionResult Index()
        {
            var userId = SessionConfig.GetUserId();
            var orders = _db.Orders
                .Where(o => o.userId == userId)
                .ToList();

            return View(orders);
        }

        public ActionResult Delete(int id)
        {
            var ds = _db.Orders.Find(id);
            _db.Orders.Remove(ds);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

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

                if (order.status == "Processing")
                {
                    return Json(new { success = false, message = "Đơn hàng đã được thanh toán" });
                }

                Debug.WriteLine($"Creating payment request for order {orderId} with amount {order.finalAmount}");

                // Generate a unique requestId
                var requestId = Guid.NewGuid().ToString();
                var uniqueOrderId = $"{orderId}_{requestId}";
                var paymentUrl = await _momoService.CreatePaymentRequest(
                    uniqueOrderId,
                    order.finalAmount,
                    $"Thanh toán đơn hàng #{orderId}"
                );

                Debug.WriteLine($"Payment URL received: {paymentUrl}");

                // Return the paymentUrl to the frontend
                return Json(new { success = true, paymentUrl = paymentUrl, requestId = requestId, orderId = uniqueOrderId });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in PayWithMomo: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> CheckPaymentStatus(int orderId, string requestId, string uniqueOrderId)
        {
            try
            {
                var order = _db.Orders.Find(orderId);
                if (order == null)
                {
                    Debug.WriteLine($"Order {orderId} not found in database");
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                if (order.status == "Processing")
                {
                    Debug.WriteLine($"Order {orderId} is already in Processing status");
                    return Json(new { success = true, message = "Đơn hàng đã được thanh toán" });
                }

                Debug.WriteLine($"Checking payment status for order {orderId}, uniqueOrderId: {uniqueOrderId}, requestId: {requestId}");

                // Query the transaction status
                var (success, message) = await _momoService.QueryTransaction(uniqueOrderId, requestId);

                Debug.WriteLine($"QueryTransaction result for order {orderId}: success={success}, message={message}");

                if (success)
                {
                    // Update order status
                    order.status = "Processing";
                    Debug.WriteLine($"Updated order {orderId} status to Processing");

                    // Save payment information to the database
                    var payment = new Payment
                    {
                        orderId = order.orderId,
                        paidAmount = order.finalAmount,
                        transactionId = "MOMO_" + Guid.NewGuid().ToString(),
                        paymentStatus = "completed",
                        paymentMethod = "paypal",
                        paidDate = DateTime.Now
                    };

                    Debug.WriteLine($"Adding payment for order {orderId}: orderId={payment.orderId}, paidAmount={payment.paidAmount}, transactionId={payment.transactionId}, paymentStatus={payment.paymentStatus}, paymentMethod={payment.paymentMethod}, paidDate={payment.paidDate}");

                    _db.Payments.Add(payment);

                    try
                    {
                        _db.SaveChanges();
                        Debug.WriteLine($"Order {orderId} marked as paid and payment saved successfully");
                    }
                    catch (Exception saveEx)
                    {
                        Debug.WriteLine($"Error saving payment for order {orderId}: {saveEx.Message}");
                        Debug.WriteLine($"Stack Trace: {saveEx.StackTrace}");
                        throw;
                    }

                    return Json(new { success = true, message = "Thanh toán thành công" });
                }
                else
                {
                    Debug.WriteLine($"Payment not successful for order {orderId}: {message}");
                    return Json(new { success = false, message = message });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in CheckPaymentStatus for order {orderId}: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Handle MOMO callback (IPN - Instant Payment Notification)
        [HttpPost]
        public ActionResult PaymentCallBack()
        {
            // This action can be used to handle MOMO's IPN notifications
            // For now, we'll just log the callback and return a success response
            Debug.WriteLine("Received MOMO IPN callback");
            return new HttpStatusCodeResult(200);
        }

        // Handle MOMO return URL
        public ActionResult ReturnFromMomo(string orderId, string resultCode, string message)
        {
            if (resultCode == "0")
            {
                TempData["SuccessMessage"] = "Thanh toán thành công!";
                return RedirectToAction("~/Customer/Order/Index");
            }
            else
            {
                TempData["ErrorMessage"] = $"Thanh toán thất bại: {message}";
            }

            return RedirectToAction("Index");
        }
    }
}