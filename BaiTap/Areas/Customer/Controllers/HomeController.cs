using BaiTap.App_Start;
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

        public ActionResult Index(int? categoryId)
        {
            var products = _db.Products.AsQueryable();

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.categoryId == categoryId.Value);
            }

            ViewBag.Categories = _db.Categories.ToList(); 
            return View(products.ToList());
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