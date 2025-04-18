using BaiTap.App_Start;
using BaiTap.Models;
using Dynamitey.DynamicObjects;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
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
            var Cart = _db.Carts
            .Where(c => c.userId == userId)
            .ToList();
            return View(Cart);
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
                    createdAt = System.DateTime.Now
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
        public ActionResult OrderConfirmation(int orderId)
        {
            var order = _db.Orders
                .Include(o => o.OrderDetails.Select(od => od.Product))
                .FirstOrDefault(o => o.orderId == orderId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Đơn hàng không tồn tại!";
                return RedirectToAction("Cart");
            }

            return View(order);
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
                .Where(c => c.userId == userId && productIds.Contains(c.productId))
                .ToList();

            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm trong giỏ hàng";
                return RedirectToAction("Cart");
            }

            var newOrder = new Order
            {
                userId = userId.Value,
                orderDate = DateTime.Now,
                totalAmount = cartItems.Sum(c => c.quantity * c.Product.price),
                finalAmount = cartItems.Sum(c => c.quantity * c.Product.price),
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
            _db.SaveChanges();
            return RedirectToAction("OrderConfirmation", new { orderId = newOrder.orderId });

        }

    }

}
