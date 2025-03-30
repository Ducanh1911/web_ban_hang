
using BaiTap.Areas.Admin.Servive;
using BaiTap.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Razor.Tokenizer.Symbols;
using PagedList;


namespace BaiTap.Areas.Admin.Controllers
{
    public class ProductController : Controller
    {
        public readonly ShopEntities _db;
        private readonly ProductService _productService;
        public ProductController(ShopEntities db, ProductService productService)
        {
            _db = db;
            _productService = productService;
        }
        public ActionResult LoadProduct(int? page, string search = "")
        {
            int pageSize = 5;
            int pageNumber = (page ?? 1);

            var products = _productService.GetProducts()
                            .Where(p => p.productName.Contains(search) || string.IsNullOrEmpty(search))
                            .OrderBy(p => p.productId)
                            .ToPagedList(pageNumber, pageSize);

            ViewBag.Search = search;
            return View(products);
        }
        //upload file
        public string UploadFile(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                var newFileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                string fileName = Path.GetFileName(newFileName);
                string filePath = Path.Combine(Server.MapPath("~/Img"), fileName);

                file.SaveAs(filePath);

                return "/Img/" + fileName;
            }
            return string.Empty;
        }

        [HttpPost]
        public ActionResult Add(Product product, HttpPostedFileBase file)
        {
            string imageUrl = UploadFile(file);
            if (!string.IsNullOrEmpty(imageUrl))
            {
                product.imageUrl = imageUrl;
            }

            if (_productService.Add(product) == true)
            {
                TempData["AddSuccessMessage"] = "Thêm sản phẩm thành công!";
                return Redirect("~/Admin/Product/LoadProduct");

            }
            else
            {
                return View(product);
            }
        }
        public ActionResult Edit(int id)
        {
            return View(_productService.Detail(id));
        }
        [HttpPost]

        public ActionResult Edit(Product product, HttpPostedFileBase file)
        {

            if (file != null && file.ContentLength > 0)
            {
                string imageUrl = UploadFile(file);
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    product.imageUrl = imageUrl;
                }
            }
            if (_productService.Update(product) == true)
            {
                TempData["EditSuccessMessage"] = "Cập nhật sản phẩm thành công!";
                return Redirect("~/Admin/Product/LoadProduct");
            }
            else
            {
                return View(product);
            }
        }
        public ActionResult Delete(int id)
        {
            _productService.Delete(id);
            TempData["EditSuccessMessage"] = "Xoá sản phẩm thành công!";
            return Redirect("~/Admin/Product/LoadProduct");
        }
    }
}











