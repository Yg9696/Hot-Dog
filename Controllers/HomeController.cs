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
            return View("Admin");
        }
        public IActionResult Index()
        {
            string userJson = HttpContext.Session.GetString("CurrentAccount");
            currentAccount = null;

            if (!string.IsNullOrEmpty(userJson))
            {
                currentAccount = JsonConvert.DeserializeObject<AccountModel>(userJson);
            }
            if (currentAccount.Age != "") { 
            list = shop.GetListOf("Products").Cast<ProductsModel>().ToList().Where(p => p.AgeLimit <= Convert.ToInt32(currentAccount.Age)).ToList();
        }
            else{
                list = shop.GetListOf("Products").Cast<ProductsModel>().ToList();
            }
            return View(list);
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
