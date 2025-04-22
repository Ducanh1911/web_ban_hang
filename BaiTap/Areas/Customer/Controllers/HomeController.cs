using BaiTap.App_Start;
using BaiTap.Areas.Admin.Servive;
using BaiTap.Models;
using PagedList;
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

        public ActionResult Index(int? page, string search = "", int? categoryId = null)
        {
            int pageSize = 8; // Number of products per page
            int pageNumber = (page ?? 1); // Default to page 1 if not specified

            // Get products with optional search and category filtering
            var products = _db.Products
                .Where(p => (string.IsNullOrEmpty(search) || p.productName.ToLower().Contains(search.ToLower()))
                         && (!categoryId.HasValue || p.categoryId == categoryId))
                .OrderBy(p => p.productId);

            // Convert to PagedList
            var pagedProducts = products.ToPagedList(pageNumber, pageSize);

            // Pass categories for sidebar
            ViewBag.Categories = _db.Categories.ToList();
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;

            return View(pagedProducts);
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