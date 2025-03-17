using BaiTap.Models;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BaiTap.Areas.Customer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ShopEntities _db;
        public HomeController(ShopEntities db)
        {
            _db = db;
        }       
        // GET: Customer/Home
        public ActionResult Index()
        {
            return View(_db.Products.ToList());
        }
        public ActionResult Detail(int id)
        {
            var product = _db.Products.FirstOrDefault(p => p.productId == id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }
    }
}