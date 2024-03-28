using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ShopProject.Models;
using ShopProject.Services;
using System;
using System.Collections;
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
        private List<ProductsModel> list;
        private AccountModel currentAccount;
        public string currentCollection;
        private readonly IHttpContextAccessor _httpContextAccessor;



        public ProductsController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            sign = true;
            _configuration = configuration;
            connectionString = configuration.GetConnectionString("myConnect");
            shop = new ShopService(_configuration);
            string userJson = _httpContextAccessor.HttpContext.Session.GetString("CurrentAccount");
            currentAccount = null;

            if (!string.IsNullOrEmpty(userJson))
            {
                currentAccount = JsonConvert.DeserializeObject<AccountModel>(userJson);
            }
            if (currentAccount.Age != "")
            {
                list = shop.GetListOf("Products").Cast<ProductsModel>().ToList().Where(p => p.AgeLimit <= Convert.ToInt32(currentAccount.Age)).ToList();
            }
            else
            {
                list = shop.GetListOf("Products").Cast<ProductsModel>().ToList();
            }
        }
        public IActionResult AllProducts()
        {
            return View("MyProducts");
        }
        public IActionResult Item()
        {
            return View("item");
        }



        public IActionResult Cart()
        {
            string userJson = HttpContext.Session.GetString("CurrentAccount");
            AccountModel currentAccount = null;

            if (!string.IsNullOrEmpty(userJson))
            {
                currentAccount = JsonConvert.DeserializeObject<AccountModel>(userJson);
            }
            List<ProductsModel> listTemp = new List<ProductsModel>();


            if (currentAccount != null)
            {
                var shopListProductIds = shop.GetListOf("ShopList")
                             .Where(p => int.Parse(p.UserId) == currentAccount.UserID)
                             .Select(p => int.Parse(p.ProductId))
                             .ToList();


                foreach (dynamic p in shopListProductIds)
                {
                    listTemp.Add(list.Find(product => product.ProductId == p));
                }

            }
            return View("cart", listTemp);
        }

        public IActionResult AddToCart(int productId, string productList)
        {

            string userJson = HttpContext.Session.GetString("CurrentAccount");
            AccountModel currentAccount = null;

            if (!string.IsNullOrEmpty(userJson))
            {
                currentAccount = JsonConvert.DeserializeObject<AccountModel>(userJson);
            }
            shop.AddItemTo(new { UserId = currentAccount.UserID, ProductId = productId }, "ShopList");
            ProductsModel product = (ProductsModel)shop.GetItemById("Products", productId);
            product.Stock--;
            shop.UpdateItemFrom(product, "Products");
            currentCollection = HttpContext.Session.GetString("CurrentCollection");
            if (currentCollection != null)
            {
                return RedirectToAction("ProductsCollection", new { collection = currentCollection });
            }
            return View("MyProducts", list);
        }


        public IActionResult AddProduct()
        {;
            ProductsModel product = new ProductsModel();
            return View(product);
        }
        [HttpPost]
        public IActionResult MyProducts(ProductsModel product)
        {
            product.DateReliesed = DateTime.Now.Date;
            if (product != null)
            {
                shop.AddItemTo(product, "Products");
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


        }
        [HttpPost]
        public IActionResult SortProducts(string selectedFilter, string sign, int minPrice, int maxPrice)
        {
            currentCollection = HttpContext.Session.GetString("CurrentCollection");
            List<ProductsModel> listTemp = list;
            if (currentCollection != null)
            {
                
                listTemp = list.Where(p => p.Collection == currentCollection).ToList();
            }
            if (selectedFilter != null) {
                if (sign == "up")
                {

                    switch (selectedFilter)
                    {
                        case "name":
                            listTemp = listTemp.OrderBy(l => l.ProductName).ToList();
                            break;
                        case "price":
                            listTemp = listTemp.OrderBy(l => l.Price).ToList();
                            break;
                        case "collection":
                            listTemp = listTemp.OrderBy(l => l.Collection).ToList();
                            break;
                        case "stock":
                            listTemp = listTemp.OrderBy(l => l.Stock).ToList();
                            break;
                        case "Popularity":
                            listTemp = listTemp.OrderBy(l => l.NumOfOrders).ToList();
                            break;
                        case "DateModified":
                            listTemp = listTemp.OrderBy(l => l.DateReliesed.Ticks).ToList();
                            break;




                    }
                }
                else
                {
                    switch (selectedFilter)
                    {
                        case "name":
                            listTemp = listTemp.OrderByDescending(l => l.ProductName).ToList();
                            break;
                        case "price":
                            listTemp = listTemp.OrderByDescending(l => l.Price).ToList();
                            break;
                        case "collection":
                            listTemp = listTemp.OrderByDescending(l => l.Collection).ToList();
                            break;
                        case "stock":
                            listTemp = listTemp.OrderByDescending(l => l.Stock).ToList();
                            break;
                        case "Popularity":
                            listTemp = listTemp.OrderByDescending(l => l.NumOfOrders).ToList();
                            break;
                        case "DateModified"://dosent work
                            listTemp = listTemp.OrderByDescending(l => l.DateReliesed.Ticks).ToList();
                            break;

                    }
                }
            }
            if(sign== "Search")
            {
                if (minPrice != 0|| maxPrice > 0)
                {
                    if(minPrice>maxPrice)
                    {
                        TempData["AlertMessage"] = "Input error -min price can't be higher then max price";
                    }
                    else
                    {

                         listTemp = list.Where(l =>
                         (l.Price * (Convert.ToSingle(100 - l.Discount) / 100)) <= maxPrice &&
                         (l.Price * (Convert.ToSingle(100 - l.Discount) / 100)) >= minPrice
                     ).ToList();

                    }
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
            HttpContext.Session.SetString("CurrentCollection", collection);
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
