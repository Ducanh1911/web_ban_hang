using BaiTap.App_Start;
using BaiTap.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BaiTap.Areas.Customer.Controllers
{
    public class OrderController : Controller
    {
        private readonly ShopEntities _db;
        public OrderController(ShopEntities db)
        {
            _db = db;
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
            return RedirectToAction("index");
        }
    }
}