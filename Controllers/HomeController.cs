using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using ShopProject.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ShopProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ShopService shop;
        private readonly IConfiguration _configuration;
        private readonly string connectionString;
        private List<ProductsModel> list;
        private AccountModel currentAccount;
        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _configuration = configuration;
            shop = new ShopService(_configuration);
            _logger = logger;
            
        }
        public IActionResult About()
        {
            return View("About");
        }
        public IActionResult Admin()
        {

            return View("Admin", shop.GetListOf("Products").Cast<ProductsModel>().ToList());
        }
        public IActionResult Index()
        {
            string userJson = HttpContext.Session.GetString("CurrentAccount");
            currentAccount = null;
            List<ProductsModel> TempList;
            if (!string.IsNullOrEmpty(userJson))
            {
                currentAccount = JsonConvert.DeserializeObject<AccountModel>(userJson);
            }
            if (currentAccount.Age != "") {
                TempList = shop.GetListOf("Products").Cast<ProductsModel>().ToList().Where(p => p.AgeLimit <= Convert.ToInt32(currentAccount.Age)).ToList();
        }
            else{
                TempList = shop.GetListOf("Products").Cast<ProductsModel>().ToList();
            }

            var listOfIds = shop.GetListOf("NotifyList").ToList().Where(p => (p.UserId) == (currentAccount.UserID).ToString() && shop.GetItemById("Products", int.Parse(p.ProductId)).Stock > 0);
            List<ProductsModel> notifyList = new List<ProductsModel>();
            foreach (dynamic p in listOfIds)
            {
                notifyList.Add(TempList.Find(product => product.ProductId == int.Parse(p.ProductId)));
            }
            if (notifyList != null)
            {
                var jsonString = System.Text.Json.JsonSerializer.Serialize(notifyList);
                HttpContext.Session.SetString("NotifyList", jsonString);
            }
            return View(TempList);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

