using BaiTap.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BaiTap.Areas.Admin.Controllers
{
    public class OrderDetailController : Controller
    {
        // GET: Admin/Order
        private readonly ShopEntities _db;
        public OrderDetailController(ShopEntities db) {
            _db = db;
        }
        public ActionResult Index()
        {
            return View(_db.OrderDetails.ToList());
        }
    }
}