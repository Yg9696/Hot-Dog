using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
//using Oracle.ManagedDataAccess.Client; // If using Managed ODP.NET

namespace ShopProject.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View("LoginPage");
        }
    }
}

