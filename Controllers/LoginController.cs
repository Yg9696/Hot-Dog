using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ShopProject.Models;
//using Oracle.ManagedDataAccess.Client; // If using Managed ODP.NET

namespace ShopProject.Controllers
{

    public class LoginController : Controller
    {
        private readonly IConfiguration _configuration;
        string connectionString = "";
        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("myConnect");

        }
        public IActionResult Index()
        {
            UsersModel user = new UsersModel();
            return View("LoginPage", user);
        }
        [Route("ShowDetails")]
        public IActionResult ShowDetails(UsersModel user)
        {
            return View("ShowDetails", user);
        }
    }
}

