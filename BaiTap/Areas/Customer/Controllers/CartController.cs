using BaiTap.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
            return View(_db.Carts.ToList());
        }
    // Thêm sản phẩm vào giỏ hàng
    public ActionResult AddToCart(int id)
        {
            // Kiểm tra sản phẩm có tồn tại trong cơ sở dữ liệu không
            var product = _db.Products.FirstOrDefault(p => p.productId == id);
            if (product == null)
            {
                return HttpNotFound();
            }

            // Kiểm tra xem người dùng đã đăng nhập hay chưa (nếu có)

            // Kiểm tra giỏ hàng của người dùng trong database
            var cartItem = _db.Carts.FirstOrDefault(c => c.productId == id );

            if (cartItem == null)
            {
                // Nếu chưa có sản phẩm này trong giỏ, thêm mới
                _db.Carts.Add(new Cart
                {
                    productId = product.productId,
                    quantity = 1,
                });
            }
            else
            {
                // Nếu sản phẩm đã có trong giỏ hàng, tăng số lượng
                cartItem.quantity++;
            }

            // Lưu giỏ hàng vào cơ sở dữ liệu
            _db.SaveChanges();

            return RedirectToAction("Index");
        }
        // Xóa sản phẩm khỏi giỏ hàng
        public ActionResult Delete(int id)
        {
            var cartItem = _db.Carts.FirstOrDefault(c => c.cartId == id);
            if (cartItem != null)
            {
                _db.Carts.Remove(cartItem);
                _db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

    }

        

    }


