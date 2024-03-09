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
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using ShopProject.Services;
//using Oracle.ManagedDataAccess.Client; // If using Managed ODP.NET

namespace ShopProject.Controllers
{

    public class LoginController : Controller
    {
        private readonly IConfiguration _configuration;
        string connectionString = "";
        ShopService shop;
        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("myConnect");
            shop = new ShopService(_configuration);

        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return View("LoginPage");
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
                if (!UserExistsInDatabase(user.UserName,user.Password))
                {
                    //UserRegister reg = new UserRegister();
                    // AddAccountToDataBase(reg.UserID, reg.FirstName, reg.LastName, reg.UserName, reg.Password, reg.Email, reg.PhoneNumber);
                    //return View("REGISTER");
                    return View("Register");
                }
                else
                {
                    AccountModel currentAccount=shop.GetListOf("Accounts").Cast<AccountModel>().ToList().FirstOrDefault(p=>p.UserName == user.UserName);///here
                    HttpContext.Session.SetString("CurrentAccount", JsonConvert.SerializeObject(currentAccount));
                    return View("LoginSucceed", currentAccount);
                }

            }
        }
        //register action
            public IActionResult Register(AccountModel user)
            {
                if (ModelState.IsValid)
                {
                    // Call method to add account to database
                    AddAccountToDataBase(user.UserID, user.FirstName, user.LastName, user.Email, user.PhoneNumber, user.UserName, user.Password);

                // Redirect user to login page or any other page
                //return RedirectToAction("Index");
                return View("LoginPage");
                }
                else
                {
                    // If model state is not valid, return to registration page with validation errors
                    return View("Register", user);
                }
            }
        

        public IActionResult REG()
        {
            return View("Register");
        }

        //method to add the new username and password to data base
        public void AddUserToDataBase(string username, string password)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQueryInit = $"SELECT CASE WHEN EXISTS (SELECT * FROM sys.tables WHERE name = 'Users' AND schema_id = SCHEMA_ID('dbo')) THEN 1 ELSE 0 END";
                string sqlQuery = "INSERT INTO Users VALUES(@value1,@value2)";
                using (SqlCommand command = new SqlCommand(sqlQueryInit, connection))
                {
                    if ((int)command.ExecuteScalar() == 0)
                    {
                        sqlQueryInit = $"CREATE TABLE Users(Id INT PRIMARY KEY IDENTITY,UserName VARCHAR(30), Password VARCHAR(30))";
                        command.CommandText = sqlQueryInit;
                        command.ExecuteNonQuery();
                    }

                }
                        using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@value1", username);
                    command.Parameters.AddWithValue("@value2", password);
                    int rowsAffected = command.ExecuteNonQuery();
                }
            }
        }
        public void AddAccountToDataBase( int id, string name,string lastname,string email,string phone,string username,string password)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQueryInit = $"SELECT CASE WHEN EXISTS (SELECT * FROM sys.tables WHERE name = 'Accounts' AND schema_id = SCHEMA_ID('dbo')) THEN 1 ELSE 0 END";
                string sqlQuery = "INSERT INTO ACCOUNTS VALUES(@value2,@value3,@value4,@value5,@value6,@Value7)";
                using (SqlCommand command = new SqlCommand(sqlQueryInit, connection))
                {
                    if ((int)command.ExecuteScalar() == 0)
                    {
                        sqlQueryInit = $"CREATE TABLE Accounts(Id INT PRIMARY KEY IDENTITY,name VARCHAR(30)," +
                            $" lastname VARCHAR(30),username VARCHAR(30),password VARCHAR(30),email VARCHAR(30),phone VARCHAR(30))";
                        command.CommandText = sqlQueryInit;
                        command.ExecuteNonQuery();
                    }

                }
             
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                   
                    command.Parameters.AddWithValue("@value2", name);
                    command.Parameters.AddWithValue("@value3", lastname);
                    command.Parameters.AddWithValue("@value4", username);
                    command.Parameters.AddWithValue("@value5", password);
                    command.Parameters.AddWithValue("@value6", email);
                    command.Parameters.AddWithValue("@value7", phone);
                    int rowsAffected = command.ExecuteNonQuery();
                }
            }
            AddUserToDataBase(username, password);
        }

        //method to check if the username exsist in the database
        private bool UserExistsInDatabase(string username,string password)
        {
            bool userExists = false;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = $"SELECT CASE WHEN EXISTS (SELECT * FROM sys.tables WHERE name = 'Users' AND schema_id = SCHEMA_ID('dbo')) THEN 1 ELSE 0 END";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    if ((int)command.ExecuteScalar() == 0)
                    {
                        return false;
                    }
                   sqlQuery = "SELECT COUNT(*) FROM USERS WHERE UserName = @username AND Password=@password";

                    command.CommandText=sqlQuery;
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);
                    int count = (int)command.ExecuteScalar();

                    if (count > 0)
                    {
                        userExists = true;
                    }
                }
            }

            return userExists;
        }

    }
}

