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

namespace ShopProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ShopService shop;
        private readonly IConfiguration _configuration;
        private readonly string connectionString;
        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _configuration = configuration;
            shop = new ShopService(_configuration);
            _logger = logger;
        }

        public IActionResult Index()
        {
           
            return View(shop.GetListOf("Products").Cast<ProductsModel>().ToList());
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
