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
            {//sql
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sqlQuery = "INSERT INTO USERS VALUES(@value1,@value2)";
                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@value1", user.UserName);
                        command.Parameters.AddWithValue("@value2", user.Password);
                        int rowsAffected = command.ExecuteNonQuery();
                        
                        return View("ShowDetails", user);
                        
                        

                    }
                }

            }
            return View("ShowDetails", user);
        }
    }
}

