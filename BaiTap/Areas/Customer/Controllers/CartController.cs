using BaiTap.App_Start;
using BaiTap.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BaiTap.Areas.Customer.Controllers
{
    [RoleUser]
    public class CartController : Controller
    {
        private readonly ShopEntities _db;

        public CartController(ShopEntities db)
        {
            _db = db;
        }

        public ActionResult Cart()
        {
            var userId = SessionConfig.GetUserId();
            if (userId == null)
            {
                return Redirect("~/User/Login");
            }
            var cart = _db.Carts
                .Include(c => c.Product)
                .Where(c => c.userId == userId)
                .ToList();
            return View(cart);
        }

        [HttpPost]
        public ActionResult AddToCart(int productId, int quantity)
        {
            var userId = SessionConfig.GetUserId();
            if (userId == null)
            {
                return Redirect("~/User/Login");
            }
            var cartItem = _db.Carts.FirstOrDefault(c => c.userId == userId && c.productId == productId);

            if (cartItem != null)
            {
                cartItem.quantity += quantity;
            }
            else
            {
                var newCartItem = new Cart
                {
                    userId = userId,
                    productId = productId,
                    quantity = quantity,
                    createdAt = DateTime.Now
                };
                _db.Carts.Add(newCartItem);
            }
            _db.SaveChanges();
            TempData["AddMessage"] = "Thêm thành công vào giỏ hàng!";
            return Redirect("~/Customer/Home/Index");
        }

        [HttpPost]
        public ActionResult RemoveCart(int productId)
        {
            var userId = SessionConfig.GetUserId();
            var cartItem = _db.Carts.FirstOrDefault(c => c.userId == userId && c.productId == productId);

            if (cartItem != null)
            {
                _db.Carts.Remove(cartItem);
                _db.SaveChanges();
            }

            return RedirectToAction("Cart");
        }

        [HttpPost]
        public ActionResult UpdateQuantity(int productId, int change)
        {
            var userId = SessionConfig.GetUserId();
            var cartItem = _db.Carts.FirstOrDefault(c => c.userId == userId && c.productId == productId);
            if (cartItem != null)
            {
                cartItem.quantity = Math.Max(1, cartItem.quantity + change);
                _db.SaveChanges();
            }
            return RedirectToAction("Cart");
        }

        [HttpPost]
        public ActionResult Checkout(string selectedProductIds)
        {
            var userId = SessionConfig.GetUserId();
            if (userId == null)
            {
                return Redirect("~/User/Login");
            }

            var productIds = selectedProductIds
                .Split(',')
                .Where(id => int.TryParse(id, out _))
                .Select(int.Parse)
                .ToList();

            var cartItems = _db.Carts
                .Include(c => c.Product)
                .Where(c => c.userId == userId && productIds.Contains(c.productId))
                .ToList();

            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm trong giỏ hàng.";
                return RedirectToAction("Cart");
            }

            var user = _db.Users.FirstOrDefault(u => u.userId == userId) ?? new User();
            var model = new Tuple<List<Cart>, User>(cartItems, user);

            ViewBag.SelectedProductIds = selectedProductIds;

            return View("Checkout", model);
        }

        [HttpPost]
        public ActionResult ConfirmCheckout(string selectedProductIds, string fullName, string phoneNumber, string address)
        {
            var userId = SessionConfig.GetUserId();
            if (userId == null)
            {
                return Redirect("~/User/Login");
            }

            // Validate form inputs
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(phoneNumber) || string.IsNullOrEmpty(address))
            {
                TempData["ErrorMessage"] = "Vui lòng điền đầy đủ thông tin giao hàng.";
                return RedirectToAction("Checkout", new { selectedProductIds });
            }

            if (!Regex.IsMatch(phoneNumber, @"^[0-9]{10}$"))
            {
                TempData["ErrorMessage"] = "Số điện thoại phải có 10 chữ số.";
                return RedirectToAction("Checkout", new { selectedProductIds });
            }

            // Lấy user từ database để cập nhật
            var user = _db.Users.FirstOrDefault(u => u.userId == userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Checkout", new { selectedProductIds });
            }

            // Cập nhật thông tin user
            user.fullName = fullName;
            user.phoneNumber = phoneNumber;
            user.address = address;

            // Chuyển đổi selectedProductIds sang List<int>
            var productIds = selectedProductIds
                .Split(',')
                .Where(id => int.TryParse(id, out _))
                .Select(int.Parse)
                .ToList();

            // Lấy sản phẩm trong giỏ hàng
            var cartItems = _db.Carts
                .Include(c => c.Product)
                .Where(c => c.userId == userId && productIds.Contains(c.productId))
                .ToList();

            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm trong giỏ hàng.";
                return RedirectToAction("Cart");
            }

            // Tạo đơn hàng mới
            var newOrder = new Order
            {
                userId = userId.Value,
                orderDate = DateTime.Now,
                totalAmount = cartItems.Sum(c => c.quantity * c.Product.price),
                finalAmount = cartItems.Sum(c => c.quantity * c.Product.price),
                discountAmount = 0,
                status = "Pending",
                OrderDetails = cartItems.Select(c => new OrderDetail
                {
                    productId = c.productId,
                    quantity = c.quantity,
                    price = c.Product.price,
                    subtotal = c.quantity * c.Product.price
                }).ToList()
            };

            _db.Orders.Add(newOrder);
            _db.Carts.RemoveRange(cartItems);

            try
            {
                _db.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var eve in ex.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Property: {ve.PropertyName} Error: {ve.ErrorMessage}");
                    }
                }
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi lưu đơn hàng.";
                return RedirectToAction("Checkout", new { selectedProductIds });
            }

            return RedirectToAction("OrderConfirmation", new { orderId = newOrder.orderId });
        }

        public ActionResult OrderConfirmation(int orderId)
        {
            var order = _db.Orders
                .Include(o => o.OrderDetails.Select(od => od.Product))
                .Include(o => o.User)
                .FirstOrDefault(o => o.orderId == orderId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Đơn hàng không tồn tại!";
                return RedirectToAction("Cart");
            }

            return View(order);
        }
    }
}