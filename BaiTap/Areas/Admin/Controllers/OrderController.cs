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
    }
}