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
using ShopProject.Services;

using Microsoft.AspNetCore.Http;
using System.Security.Principal;
using System.Net;
using Newtonsoft.Json;

namespace ShopProject.Controllers
{

    public class LoginController : Controller
    {
        private readonly IConfiguration _configuration;
        string connectionString = "";
        private static readonly Random random = new Random();
        //
        private readonly ShopService shop;
        List<ProductsModel> list;
        //
        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("myConnect");
            //
            shop = new ShopService(_configuration);
            list = shop.GetListOf("Products").Cast<ProductsModel>().ToList();
            //
        }
       

        

       
        public IActionResult Home(UsersModel User)
        {
            UsersModel user = new UsersModel();
            return View("HomePage", User);
        }
        public IActionResult Logout()
        {
            string userJson = HttpContext.Session.GetString("CurrentAccount");
            AccountModel currentAccount = null;

            if (!string.IsNullOrEmpty(userJson))
            {
                currentAccount = JsonConvert.DeserializeObject<AccountModel>(userJson);
            }
            if (currentAccount.FullAddress != null)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = $"UPDATE Accounts SET name = @value1,lastname = @value2,age = @value3,email = @value4,phone = @value5,FullAddress = @value6 WHERE Id = @value7";
                    SqlCommand command = new SqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@value1", currentAccount.FirstName);
                    command.Parameters.AddWithValue("@value2", currentAccount.LastName);
                    command.Parameters.AddWithValue("@value3", currentAccount.Age);
                    command.Parameters.AddWithValue("@value4", currentAccount.Email);
                    command.Parameters.AddWithValue("@value5", currentAccount.PhoneNumber);
                    command.Parameters.AddWithValue("@value6", currentAccount.FullAddress);
                    command.Parameters.AddWithValue("@value7", currentAccount.UserID);
                    command.ExecuteNonQuery();
                }
            }
            HttpContext.Session.Remove("CurrentAccount");
            return RedirectToAction("Index","Login");
        }


        public IActionResult Index()
        {
            
            return View("LoginPage");
        }
        [Route("ShowDetails")]
        public IActionResult ShowDetails(UsersModel user, bool IsAdmin)
        {
            {
                if (IsAdmin)
                {
                    if (AdminIsExsistInDataBase(user.UserName, user.Password))
                    {
                        var account = shop.GetListOf("Accounts").Cast<AccountModel>().FirstOrDefault(p => p.UserName == user.UserName);
                        if (account != null)
                        {
                            var jsonString = System.Text.Json.JsonSerializer.Serialize(account);
                            HttpContext.Session.SetString("CurrentAccount", jsonString);
                        }
                        return RedirectToAction("Admin", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid username or password for admin.");
                        return View("LoginPage", user);
                    }
                }
                else
                {
                    if (!UserExistsInDatabase(user.UserName, user.Password))
                    {

                        return View("Register");
                    }
                    else
                    {
                        var account = shop.GetListOf("Accounts").Cast<AccountModel>().FirstOrDefault(p => p.UserName == user.UserName);
                        if (account != null)
                        {
                            var jsonString = System.Text.Json.JsonSerializer.Serialize(account);
                            HttpContext.Session.SetString("CurrentAccount", jsonString);
                        }
                        
                    }

                    return RedirectToAction("Index","Home");
                }
            }
        }
        
            public IActionResult Register(AccountModel user)
            {
                if (ModelState.IsValid)
                {
                   
                    AddAccountToDataBase( user.FirstName, user.LastName, user.Email, user.PhoneNumber, user.UserName,user.Age, user.Password);

                
                return RedirectToAction("Index");
            }
                else
                {
                    
                    return View("Register", user);
                }
            }
        





        public IActionResult REG()
        {
            return View("Register");
        }

        public IActionResult enterAsGuest()
        {
            var jsonString = System.Text.Json.JsonSerializer.Serialize(new AccountModel
            {
                UserID = (int)(random.Next(-9999, -1000)),
                FirstName = "",
                LastName = "",
                Email = "",
                UserName = $"guest{random.Next(100, 999)}",
                Age = "",
                Password = "",
                PhoneNumber = ""
            });
            HttpContext.Session.SetString("CurrentAccount", jsonString);
            return RedirectToAction("Index", "Home");
        }
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
        public void AddAccountToDataBase(  string name,string lastname,string email,string phone,string username,string Age,string password)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQueryInit = $"SELECT CASE WHEN EXISTS (SELECT * FROM sys.tables WHERE name = 'Accounts' AND schema_id = SCHEMA_ID('dbo')) THEN 1 ELSE 0 END";
                string sqlQuery = "INSERT INTO ACCOUNTS(name,lastname,username,age,password,email,phone) VALUES(@value2,@value3,@value4,@value5,@value6,@Value7,@Value8)";
                using (SqlCommand command = new SqlCommand(sqlQueryInit, connection))
                {
                    if ((int)command.ExecuteScalar() == 0)
                    {
                        sqlQueryInit = $"CREATE TABLE Accounts(Id INT PRIMARY KEY IDENTITY,name VARCHAR(30)," +
                            $" lastname VARCHAR(30),username VARCHAR(30),age VARCHAR(30),password VARCHAR(30),email VARCHAR(30),phone VARCHAR(30),FullAddress VARCHAR(30) DEFAULT NULL,EncryptedCardNumber VARBINARY(MAX) DEFAULT NULL,   EncryptedExpiryDate VARBINARY(MAX) DEFAULT NULL,    EncryptedCVV VARBINARY(MAX) DEFAULT NULL,   IV VARBINARY(16),   KeyIdentifier NVARCHAR(50) DEFAULT NULL)";
                        command.CommandText = sqlQueryInit;
                        command.ExecuteNonQuery();
                    }

                }
             
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@value2", name);
                    command.Parameters.AddWithValue("@value3", lastname);
                    command.Parameters.AddWithValue("@value4", username);
                    command.Parameters.AddWithValue("@value5", Age);
                    command.Parameters.AddWithValue("@value6", password);
                    command.Parameters.AddWithValue("@value7", email);
                    command.Parameters.AddWithValue("@value8", phone);
                    int rowsAffected = command.ExecuteNonQuery();
                }
            }
            AddUserToDataBase(username, password);
        }

       
        private bool UserExistsInDatabase(string username,string password)
        {
            if (username == "Admin" && password == "Admin")
            {
                ModelState.AddModelError(string.Empty, "invalid login with Admin username and password");
                return false;
            }

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

        private bool AdminIsExsistInDataBase(string username, string password)
        {
            bool userExists = false;

            
            if (username == "Admin" && password == "Admin")
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    
                    string checkTableQuery = $"SELECT CASE WHEN EXISTS (SELECT * FROM sys.tables WHERE name = 'Users' AND schema_id = SCHEMA_ID('dbo')) THEN 1 ELSE 0 END";

                    using (SqlCommand command = new SqlCommand(checkTableQuery, connection))
                    {
                        if ((int)command.ExecuteScalar() == 0)
                        {
                            return false; 
                        }

                        string checkUserQuery = "SELECT COUNT(*) FROM Users WHERE UserName = @username AND Password = @password";

                        command.CommandText = checkUserQuery;
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", password);
                        int count = (int)command.ExecuteScalar();

                        if (count > 0)
                        {
                            userExists = true; 
                        }
                    }
                }
            }

            return userExists;
        }



    }
}

