using BaiTap.App_Start;
using BaiTap.Areas.Admin.Servive;
using BaiTap.Models;
using PagedList;
using System;
using System.Collections.Generic;
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
            int pageSize = 8; // Số sản phẩm trên mỗi trang
            int pageNumber = (page ?? 1); // Mặc định là trang 1 nếu không được chỉ định

            // Truy vấn cơ bản cho sản phẩm với tìm kiếm và lọc danh mục tùy chọn
            var productsQuery = _db.Products
                .Where(p => (string.IsNullOrEmpty(search) || p.productName.ToLower().Contains(search.ToLower()))
                         && (!categoryId.HasValue || p.categoryId == categoryId))
                .OrderBy(p => p.productId);

            // Chuyển đổi sang PagedList cho danh sách sản phẩm chính
            var pagedProducts = productsQuery.ToPagedList(pageNumber, pageSize);

            // Truyền danh sách danh mục cho sidebar
            ViewBag.Categories = _db.Categories.ToList();
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;

            // Lấy sản phẩm thịnh hành (không lọc theo danh mục)
            var trendingQuery = _db.Products
                .Where(p => p.price > 20000000); // Lọc sản phẩm có giá trên 20,000,000
            ViewBag.TrendingProducts = trendingQuery
                .OrderBy(p => p.price) // Sắp xếp theo giá
                .Take(6)
                .ToList();

            // Lấy sản phẩm bán chạy (lọc theo danh mục nếu được chỉ định)
            var bestSellingQuery = _db.OrderDetails
                .GroupBy(od => od.productId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQuantity = g.Sum(od => od.quantity)
                })
                .Join(_db.Products,
                    od => od.ProductId,
                    p => p.productId,
                    (od, p) => new { Product = p, TotalQuantity = od.TotalQuantity });
            if (categoryId.HasValue)
            {
                bestSellingQuery = bestSellingQuery.Where(x => x.Product.categoryId == categoryId);
            }
            ViewBag.BestSellingProducts = bestSellingQuery
                .OrderByDescending(x => x.TotalQuantity)
                .Take(6)
                .Select(x => x.Product)
                .ToList();

            // Lấy sản phẩm gợi ý (không lọc theo danh mục)
            var recommendedQuery = _db.Products.AsQueryable(); 
            ViewBag.RecommendedProducts = recommendedQuery
                .OrderBy(p => Guid.NewGuid()) 
                .Take(6)
                .ToList();

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

        public ActionResult ThanhToanThanhCong()
        {
            return View();
        }
    }
}