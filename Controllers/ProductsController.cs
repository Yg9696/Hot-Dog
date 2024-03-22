using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ShopProject.Models;
using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;


namespace ShopProject.Controllers
{

    public class ProductsController : Controller
    {
        public bool sign;
        private readonly IConfiguration _configuration;
        private readonly string connectionString;
        private readonly ShopService shop;
        List<ProductsModel> list;
        AccountModel currentAccount;
        
        

        public ProductsController(IConfiguration configuration)
        {
            sign = true;
             _configuration = configuration;
            connectionString = configuration.GetConnectionString("myConnect");
            shop = new ShopService(_configuration);
            
            list = shop.GetListOf("Products").Cast<ProductsModel>().ToList();
            
        }
        public IActionResult Item()
        {
            return View("item");
        }

        //public IActionResult Home(UsersModel User)
        //{
        //    UsersModel user = new UsersModel();
        //    return View("HomePage",User);
        //}

        public IActionResult Cart()
        {
            string userJson = HttpContext.Session.GetString("CurrentAccount");
            AccountModel currentAccount = null;

            if (!string.IsNullOrEmpty(userJson))
            {
                currentAccount = JsonConvert.DeserializeObject<AccountModel>(userJson);
            }
            List<ProductsModel> listTemp = new List<ProductsModel>();

            //listTemp=list.Where(p=>p.ProductId in shop.GetListOf("ShopList").ToList().Where(p => p.UserId == currentAccount.UserId).ToList()));
            if (currentAccount != null)
            {
                var shopListProductIds = shop.GetListOf("ShopList")
                             .Where(p => int.Parse(p.UserId) == currentAccount.UserID)
                             .Select(p =>  int.Parse(p.ProductId) )
                             .ToList();


                foreach (dynamic p  in shopListProductIds)
                {
                    listTemp.Add(list.Find(product => product.ProductId == p));
                }
                
            }
            return View("cart", listTemp);
        }
        




        public IActionResult AddToCart(int productId)
        {
             
            string userJson = HttpContext.Session.GetString("CurrentAccount");
            AccountModel currentAccount = null;

            if (!string.IsNullOrEmpty(userJson))
            {
                currentAccount = JsonConvert.DeserializeObject<AccountModel>(userJson);
            }
            shop.AddItemTo(new  {UserId= currentAccount.UserID,ProductId= productId},"ShopList");
            ProductsModel product = (ProductsModel)shop.GetItemById("Products", productId);
            product.Stock--;
            shop.UpdateItemFrom(product, "Products");

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
            product.DateReliesed= DateTime.Now.Date;
            if (product != null )
            {
                shop.AddItemTo(product,"Products");
            }
            return RedirectToAction("MyProducts");
        }
        [HttpGet]
        public IActionResult MyProducts()
        {
            string userJson = HttpContext.Session.GetString("CurrentAccount");
            AccountModel currentAccount = null;

            if (!string.IsNullOrEmpty(userJson))
            {
                currentAccount = JsonConvert.DeserializeObject<AccountModel>(userJson);
            }

            return View(shop.GetListOf("Products")
    .Cast<ProductsModel>().ToList());
   
    //.Where(p => p.AgeLimit > Convert.ToInt32(currentAccount.Age))
    //.ToList());
        }
        [HttpPost]
        public IActionResult SortProducts(string selectedFilter,string sign)
        {
            List<ProductsModel> listTemp = list;
            if (sign=="up" )
            {
                
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
                    case "Popularity":
                        listTemp = list.OrderBy(l => l.NumOfOrders).ToList();
                        break;
                    case "DateModified":
                        listTemp = list.OrderBy(l => l.DateReliesed.Ticks).ToList();
                        break;




                }
            }
            else
            {
                switch (selectedFilter)
                {
                    case "name":
                        listTemp = list.OrderByDescending(l => l.ProductName).ToList();
                        break;
                    case "price":
                        listTemp = list.OrderByDescending(l => l.Price).ToList();
                        break;
                    case "collection":
                        listTemp = list.OrderByDescending(l => l.Collection).ToList();
                        break;
                    case "stock":
                        listTemp = list.OrderByDescending(l => l.Stock).ToList();
                        break;
                    case "Popularity":
                        listTemp = list.OrderByDescending(l => l.NumOfOrders).ToList();
                        break;
                    case "DateModified"://dosent work
                        listTemp = list.OrderByDescending(l => l.DateReliesed.Ticks).ToList();
                        break;

                }
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
            return View("MyProducts",list.Where(p => p.Collection == collection).ToList());
        }
        public IActionResult deleteFromCart(int itemId)
        {
            string userJson = HttpContext.Session.GetString("CurrentAccount");
            AccountModel currentAccount = null;

            if (!string.IsNullOrEmpty(userJson))
            {
                currentAccount = JsonConvert.DeserializeObject<AccountModel>(userJson);
            }
            shop.deleteFrom(itemId,currentAccount.UserID, "ShopList");
            ProductsModel product = shop.GetItemById("Products", itemId);
            product.Stock++;
            shop.UpdateItemFrom(product, "Products");
            return RedirectToAction("Cart");
        }
    }
}
