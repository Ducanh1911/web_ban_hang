using BaiTap.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BaiTap.Areas.Admin.Repository
{
    public class ProductRepository
    {
        private readonly ShopEntities _db;
        public ProductRepository(ShopEntities db)
        {
            _db = db;
        }
        public bool add(Product t)
        {
            try{
                _db.Products.Add(t);
                _db.SaveChanges();
                return true;
            }
            catch {
                return false;
            }
        }

        public bool delete(int id)
        {
            var product= _db.Products.Find(id);
            _db.Products.Remove(product);
            _db.SaveChanges();
            return true;
        }

        public Product detail(int id)
        {

            return _db.Products.Find(id);
        }

        public bool edit(Product model)
        {
            var product = _db.Products.FirstOrDefault(m=>m.productId==model.productId);
            if (product != null)
            {
                product.productName = model.productName;
                product.description = model.description;
                product.price = model.price;
                product.discount = model.discount;
                product.stock = model.stock;
                product.categoryId = model.categoryId;
                product.imageUrl = model.imageUrl;
                product.createdAt = model.createdAt;
                _db.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }      
        }

        public List<Product> ProductList()
        {
            return _db.Products.ToList();
        }
        
    }
}