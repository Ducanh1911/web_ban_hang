using BaiTap.App_Start;
using System.Linq;
using System.Web.Mvc;
using BaiTap.Models;
using System;
using System.Data.Entity;

namespace BaiTap.Areas.Admin.Controllers
{
    [RoleUser]
    public class DashboardController : Controller
    {
        private ShopEntities db = new ShopEntities();

        // GET: Admin/Dashboard
        public ActionResult Index()
        {
            var products = db.Products.ToList();

            ViewBag.TotalProducts = products.Count();
            ViewBag.OutOfStockProducts = products.Count(p => p.stock == 0);

            var orders = db.Orders
                .Where(o => o.status != "Pending")
                .ToList(); // Lấy danh sách để debug

            ViewBag.TotalRevenue = orders.Sum(o => (decimal?)o.totalAmount) ?? 0;
            ViewBag.OrderCount = orders.Count; 

            var categoryCounts = db.Categories
                .Select(c => new
                {
                    Category = c.categoryName,
                    Count = db.Products.Count(p => p.categoryId == c.categoryId)
                }).ToList();

            ViewBag.CategoryLabels = categoryCounts.Select(x => x.Category).ToArray();
            ViewBag.CategoryData = categoryCounts.Select(x => x.Count).ToArray();

            return View(products);
        }


        // GET: Admin/Dashboard/InventoryReport
        public ActionResult InventoryReport(int? categoryId)
        {
            var products = db.Products.AsQueryable();

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.categoryId == categoryId);
            }

            var productList = products.ToList();
            ViewBag.Categories = db.Categories.ToList();
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.TotalProducts = productList.Count;
            ViewBag.OutOfStock = productList.Count(p => p.stock == 0);
            ViewBag.LowStock = productList.Count(p => p.stock > 0 && p.stock <= 10);

            return View(productList);
        }
        // GET: Admin/Dashboard/InventoryChart
        public ActionResult InventoryChart(int? categoryId)
        {
            var products = db.Products.AsQueryable();

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.categoryId == categoryId);
            }

            var productData = products
                .Select(p => new
                {
                    p.productName,
                    p.stock
                })
                .ToList();

            ViewBag.ProductNames = productData.Select(p => p.productName).ToList();
            ViewBag.StockLevels = productData.Select(p => p.stock).ToList();
           ViewBag.BackgroundColors = productData.Select(p => "#007bff").ToList();
            ViewBag.Categories = db.Categories.ToList();
            ViewBag.SelectedCategoryId = categoryId;

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}