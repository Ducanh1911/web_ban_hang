using BaiTap.App_Start;
using BaiTap.Models;
using System.Linq;
using System.Web.Mvc;

namespace BaiTap.Areas.Admin.Controllers
{
    [RoleUser]
    public class CategoryController : Controller
    {
        private readonly ShopEntities _db = new ShopEntities();

        public ActionResult Index()
        {
            var categories = _db.Categories.ToList();
            return View(categories);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                _db.Categories.Add(category);
                _db.SaveChanges();
                TempData["AddMessage"] = "Thêm danh mục thành công!";
                return RedirectToAction("Index");
            }
            return View(category);
        }
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var dmuc = _db.Categories.FirstOrDefault(c => c.categoryId == id);
            _db.Categories.Remove(dmuc);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
