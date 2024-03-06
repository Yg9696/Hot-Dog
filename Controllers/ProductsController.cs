using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ShopProject.Models;
using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace ShopProject.Controllers
{
    public class ProductsController : Controller
    {

        private readonly IConfiguration _configuration;
        private readonly string connectionString;
        private readonly ShopService shop;
        List<ProductsModel> list;
        public ProductsController(IConfiguration configuration)
        {
            _configuration = configuration;
            connectionString = configuration.GetConnectionString("myConnect");
            shop = new ShopService(_configuration);
            list = shop.GetListOf("Products").Cast<ProductsModel>().ToList();
        }

        public IActionResult AddToCart()
        {
            return View("MyProducts");
        }

        public IActionResult AddProduct()
        {;
            ProductsModel product = new ProductsModel();
            return View(product);
        }
        [HttpPost]
        public IActionResult MyProducts(ProductsModel product )
        {
            if (product != null && ModelState.IsValid)
            {
                shop.AddItemTo(product,"Products");
            }
            return RedirectToAction("MyProducts");
        }
        [HttpGet]
        public IActionResult MyProducts()
        {
        
            return View(shop.GetListOf("Products").Cast<ProductsModel>().ToList());
        }
        [HttpPost]
        public IActionResult SortProducts(string selectedFilter)
        {
            List<ProductsModel> listTemp = list;
            switch (selectedFilter)
            {
                case "name":
                    listTemp = list.OrderBy(l => l.ProductName).ToList();
                    break;
                case "price":
                    listTemp = list.OrderBy(l => l.Price).ToList();
                    break;
                case "collection":
                    listTemp = list.OrderBy(l => l.Collection).ToList();
                    break;
                case "stock":
                    listTemp = list.OrderBy(l => l.Stock).ToList();
                    break;

            }
            return View("MyProducts", listTemp);
        }
        public IActionResult FilteredProducts(string searchedInput)
        {
            List<ProductsModel> listTemp = list;
            if (!string.IsNullOrEmpty(searchedInput))
            {
                listTemp= list.Where(p => p.ProductName.Contains(searchedInput, StringComparison.OrdinalIgnoreCase) ||
                p.Description.Contains(searchedInput, StringComparison.OrdinalIgnoreCase)).ToList();
                
            }
            return View("MyProducts", listTemp);
        }
        public IActionResult ProductDetails(string productId)
        {
            ProductsModel product = list.FirstOrDefault(p => (p.ProductId).ToString() == productId);
            return View(product);
        }
    }
}
