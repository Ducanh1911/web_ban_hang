using BaiTap.App_Start;
using BaiTap.Models;
using Dynamitey.DynamicObjects;
using System;
using System.Linq;
using System.Web.Mvc;

namespace BaiTap.Areas.Customer.Controllers
{
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
            return RedirectToAction("Cart");
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

        // loi 
        public ActionResult Checkout(string selectedProductIds)
        {
            var userId = SessionConfig.GetUserId();
            if (userId == null)
            {
                return Redirect("~/User/Login");
            }

            if (string.IsNullOrEmpty(selectedProductIds))
            {
                TempData["ErrorMessage"] = "Chọn ít nhất một sản phẩm!";
                return RedirectToAction("Cart");
            }

            var productIds = selectedProductIds.Split(',').Select(int.Parse).ToList();
            var cartItems = _db.Carts
                .Where(c => c.userId == userId && productIds.Contains(c.productId))
                .Select(c => new OrderDetail
                {
                }).ToList();

            var orderViewModel = new Order { };

            return View(orderViewModel);
        }


    }

}
