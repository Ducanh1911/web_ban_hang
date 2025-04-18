using BaiTap.App_Start;
using BaiTap.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BaiTap.Areas.Admin.Controllers
{
    [RoleUser]
    public class OrderController : Controller
    {
        // GET: Admin/Order
        private readonly ShopEntities _db;
        public OrderController(ShopEntities db)
        {
            _db = db;
        }
        public ActionResult Index()
        {
            return View(_db.Orders.ToList());
        }
        public ActionResult Edit(int id)
        {
            var order = _db.Orders.FirstOrDefault(o => o.orderId == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Order model)
        {
            var order = _db.Orders.FirstOrDefault(o => o.orderId == model.orderId);
            if (order == null)
            {
                return HttpNotFound();
            }

            // Cập nhật chỉ trạng thái (vì các trường khác bị ẩn và không cho sửa)
            order.status = model.status;

            _db.SaveChanges();

            return RedirectToAction("Index");
        }

    }
}