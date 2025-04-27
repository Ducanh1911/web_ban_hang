using BaiTap.App_Start;
using BaiTap.Models;
using BaiTap.Service;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Diagnostics;
using DocumentFormat.OpenXml.Vml;

namespace BaiTap.Areas.Customer.Controllers
{
    [RoleUser]
    [RouteArea("Customer")]
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
            var order = _db.Orders.Find(id);
            _db.Orders.Remove(order);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> PayWithMomo(int orderId)
        {
            try
            {
                var order = _db.Orders.Find(orderId);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng";
                    return RedirectToAction("Index");
                }

                if (order.status != "Pending")
                {
                    TempData["ErrorMessage"] = "Đơn hàng không thể thanh toán";
                    return RedirectToAction("Index");
                }

                Debug.WriteLine($"Creating payment request for order {orderId} with amount {order.finalAmount}");

                // Generate a unique requestId and orderId for MOMO
                var requestId = Guid.NewGuid().ToString();
                var uniqueOrderId = $"{orderId}_{requestId}";
                var paymentUrl = await _momoService.CreatePaymentRequest(
                    uniqueOrderId,
                    order.finalAmount,
                    $"Thanh toán đơn hàng #{orderId}"
                );

                Debug.WriteLine($"Payment URL received: {paymentUrl}");

                // Redirect to MOMO payment page
                return Redirect(paymentUrl);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in PayWithMomo: {ex.Message}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo thanh toán: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<ActionResult> PaymentCallBack()
        {
            try
            {
                Debug.WriteLine("Received MOMO IPN callback");

                // Extract parameters from the request
                var requestBody = Request.Form;
                var orderId = requestBody["orderId"];
                var resultCode = requestBody["resultCode"];
                var transId = requestBody["transId"];
                var amount = decimal.Parse(requestBody["amount"]);
                var message = requestBody["message"];

                Debug.WriteLine($"IPN Data - orderId: {orderId}, resultCode: {resultCode}, transId: {transId}, amount: {amount}, message: {message}");

                if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(resultCode))
                {
                    Debug.WriteLine("Invalid IPN data received");
                    return new HttpStatusCodeResult(400);
                }

                // Extract the actual orderId (before the unique suffix)
                var actualOrderId = int.Parse(orderId.Split('_')[0]);
                var order = _db.Orders.Find(actualOrderId);

                if (order == null)
                {
                    Debug.WriteLine($"Order {actualOrderId} not found");
                    return new HttpStatusCodeResult(404);
                }

                if (resultCode == "0")
                {
                    // Payment successful
                    order.status = "Processing";
                    var payment = new Payment
                    {
                        orderId = order.orderId,
                        paidAmount = amount,
                        transactionId = transId ?? "MOMO_" + Guid.NewGuid().ToString(),
                        paymentStatus = "completed",
                        paymentMethod = "momo",
                        paidDate = DateTime.Now
                    };

                    _db.Payments.Add(payment);
                    _db.SaveChanges();

                    Debug.WriteLine($"Order {actualOrderId} updated to Processing and payment saved");
                }
                else
                {
                    Debug.WriteLine($"Payment failed for order {actualOrderId}: {message}");
                }

                return new HttpStatusCodeResult(200);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in PaymentCallBack: {ex.Message}");
                return new HttpStatusCodeResult(500);
            }
        }

        [HttpGet]
        public async Task<ActionResult> ReturnFromMomo()
        {
            try
            {
                string orderId = Request.QueryString["orderId"];
                string resultCode = Request.QueryString["resultCode"];
                string message = Request.QueryString["message"];
                string transId = Request.QueryString["transId"];
                string amount = Request.QueryString["amount"];

                Debug.WriteLine($"ReturnFromMomo - orderId: {orderId}, resultCode: {resultCode}, message: {message}, transId: {transId}, amount: {amount}");

                if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(resultCode))
                {
                    TempData["ErrorMessage"] = "Dữ liệu trả về không hợp lệ";
                    return RedirectToAction("Index");
                }

                // Extract actual orderId
                var actualOrderId = int.Parse(orderId.Split('_')[0]);
                var order = _db.Orders.Find(actualOrderId);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng";
                    return RedirectToAction("Index");
                }

                if (resultCode == "0")
                {
                    // Payment successful
                    if (order.status != "Processing")
                    {
                        order.status = "Processing";
                        var payment = new Payment
                        {
                            orderId = order.orderId,
                            paidAmount = decimal.Parse(amount),
                            transactionId = transId ?? "MOMO_" + Guid.NewGuid().ToString(),
                            paymentStatus = "completed",
                            paymentMethod = "momo",
                            paidDate = DateTime.Now
                        };

                        _db.Payments.Add(payment);
                        _db.SaveChanges();

                        Debug.WriteLine($"Order {actualOrderId} updated to Processing and payment saved in ReturnFromMomo");
                    }

                    TempData["SuccessMessage"] = "Thanh toán thành công!";
                    return RedirectToAction("ThanhToanThanhCong", "Home", new { area = "Customer" });
                }
                else
                {
                    TempData["ErrorMessage"] = $"Thanh toán thất bại: {message}";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                //lice.WriteLine($"Error in ReturnFromMomo: {ex.Message}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xử lý thanh toán";
                return RedirectToAction("Index");
            }
        }
    }
}