using BaiTap.Areas.Admin.Repository;
using BaiTap.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaiTap.Areas.Admin.Servive
{
    public class ProductService
    {
        private readonly ProductRepository _productRepository;
        public ProductService(ProductRepository repository)
        {
            _productRepository = repository;
        }
        public List<Product> GetProducts() {
            return _productRepository.ProductList();
        }
        public bool Add(Product product)
        {
            return _productRepository.add(product);
        }
        public bool Delete(int id) { 
            return _productRepository.delete(id);
        }
        public bool Update(Product product)
        {
            return _productRepository.edit(product);
        }
        public Product Detail(int id)
        {
            return _productRepository.detail(id);
        }
    }
}