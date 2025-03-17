using BaiTap.Areas.Admin.Servive;
using BaiTap.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BaiTap.Areas.Admin.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductService _productService;
        public ProductController(ProductService productService)
        {
            _productService = productService;
        }
        public ActionResult LoadProduct()
        {
            System.Diagnostics.Debug.WriteLine("LoadProduct");
            return View(_productService.GetProducts());
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

            if (_productService.Add(product) == true) {
                return Redirect("~/Admin/Product/LoadProduct");
            }
            else {
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
            return Redirect("~/Admin/Product/LoadProduct");
        }
    }
}