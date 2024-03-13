using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
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
        AccountModel currentAccount;
        public ProductsController(IConfiguration configuration)
        {
            string userJason=null;
            _configuration = configuration;
            connectionString = configuration.GetConnectionString("myConnect");
            shop = new ShopService(_configuration);
            list = shop.GetListOf("Products").Cast<ProductsModel>().ToList();
            if (HttpContext!=null) {
                userJason = HttpContext.Session.GetString("CurrentAccount");
                    }
            if (userJason != null)
            {
                currentAccount = JsonConvert.DeserializeObject<AccountModel>(userJason);
                
            }
        }

        //public IActionResult Home(UsersModel User)
        //{
        //    UsersModel user = new UsersModel();
        //    return View("HomePage",User);
        //}

        public IActionResult Cart()
        {
            return View("cart");
        }
        public void RemoveFromCart(int productId)
        {
           
            TempData["Message"] = "The product was removed successfully"; 

             
        }




        public IActionResult AddToCart(int productId)
        {
            shop.AddItemTo(new  {UserId= currentAccount.UserID,ProductId= productId},"ShopList");

            return View("MyProducts",list);
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
        [HttpPost]
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
        public IActionResult ProductDetails(int id)
        {
            
            ProductsModel product = list.FirstOrDefault(p => (p.ProductId) == id);
            return View(product);
        }
        public IActionResult ProductsCollection(string collection)
        {
            List<ProductsModel> l = list.Where(p => p.Collection == collection).ToList();
            return View("MyProducts",list.Where(p => p.Collection == collection).ToList());
        }
    }
}
