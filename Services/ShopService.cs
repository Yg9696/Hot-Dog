using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Policy;
using System.Security.Principal;
using System.Threading.Tasks;


namespace ShopProject.Services
{
    public interface IShopService
    {
        public List<dynamic> GetListOf(string listType);
        public bool AddItemTo(dynamic item, string tableName);
    }
    public class ShopService : IShopService
    {
        private readonly IConfiguration _configuration;
        private readonly string connectionString;
        


        public ShopService(IConfiguration configuration)
        {
            _configuration = configuration;
            connectionString = configuration.GetConnectionString("myConnect");
          
        }


        public List<dynamic> GetListOf(string listType)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string tableName = "";//#####
                switch (listType.ToLower())
                {
                    case "products":
                        tableName = "Products";
                        break;
                    case "shoplist":
                        tableName = "ShopList";
                        break;
                    case "accounts":
                        tableName = "Accounts";
                        break;
                    default:
                        return new List<dynamic>();
                }

                string sqlQuery = $"SELECT CASE WHEN EXISTS (SELECT * FROM sys.tables WHERE name = '{tableName}' AND schema_id = SCHEMA_ID('dbo')) THEN 1 ELSE 0 END";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    if ((int)command.ExecuteScalar() == 0)
                    {
                        return new List<dynamic>();
                    }
                }

                string sql = $"SELECT * FROM {tableName}";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    List<dynamic> items = new List<dynamic>();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dynamic item = null;
                            switch (listType.ToLower())//#####
                            {
                                case "products":
                                    item = new ProductsModel
                                    {
                                        ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                        ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                                        Price = reader.GetInt32(reader.GetOrdinal("Price")),
                                        Collection = reader.GetString(reader.GetOrdinal("Collection")),
                                        Description = reader.GetString(reader.GetOrdinal("Description")),
                                        Stock = reader.GetInt32(reader.GetOrdinal("Stock")),
                                        AgeLimit = reader.GetInt32(reader.GetOrdinal("AgeLimit")),
                                        Discount = reader.GetInt32(reader.GetOrdinal("discount")),
                                        NumOfOrders = reader.GetInt32(reader.GetOrdinal("NumOfOrders")),
                                        DateReliesed = reader.GetDateTime(reader.GetOrdinal("DateReliesed"))

                            };
                                    break;
                                case "shoplist":
                                    item = new
                                    {
                                        Id= reader.GetInt32(reader.GetOrdinal("Id")),
                                        UserId = reader.GetString(reader.GetOrdinal("UserId")),
                                        ProductId = reader.GetString(reader.GetOrdinal("ProductId"))
                                    };
                                    break;
                                case "accounts":
                                    item = new AccountModel
                                    {
                                        UserID = reader.GetInt32(reader.GetOrdinal("Id")),
                                        FirstName = reader.GetString(reader.GetOrdinal("name")),
                                        LastName = reader.GetString(reader.GetOrdinal("lastname")),
                                        UserName = reader.GetString(reader.GetOrdinal("username")),
                                        Password = reader.GetString(reader.GetOrdinal("password")),
                                        Email = reader.GetString(reader.GetOrdinal("email")),
                                        PhoneNumber = reader.GetString(reader.GetOrdinal("phone"))
                                    };
                                    break;


                            }
                            if (item != null)
                            {
                                items.Add(item);
                            }
                        }
                    }
                    return items;
                }
            }
        }
        public bool AddItemTo(dynamic item, string tableName)
        {
            int rowsAffected = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = $"SELECT CASE WHEN EXISTS (SELECT * FROM sys.tables WHERE name = '{tableName}' AND schema_id = SCHEMA_ID('dbo')) THEN 1 ELSE 0 END";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    if ((int)command.ExecuteScalar() == 0)
                    {
                        string sqlQueryInit = "";
                        switch (tableName)//#####
                        {
                            case "Products":
                                sqlQueryInit = $"CREATE TABLE {tableName}(ProductId INT PRIMARY KEY IDENTITY, ProductName VARCHAR(30), Price INT, Collection VARCHAR(100), Description VARCHAR(255), Stock INT,AgeLimit INT DEFAULT NULL ,Discount INT DEFAULT 0,NumOfOrders INT DEFAULT 0,DateReliesed DATE)";
                                break;
                            case "Users":
                                sqlQueryInit = $"CREATE TABLE {tableName}(Id INT PRIMARY KEY IDENTITY,UserName VARCHAR(30) PRIMARY KEY, Password VARCHAR(30))";
                                break;
                            case "ShopList":
                                sqlQueryInit = $"CREATE TABLE {tableName} (Id INT PRIMARY KEY IDENTITY,UserId VARCHAR(30), ProductId VARCHAR(30))"; 
                                break;

                        }

                        command.CommandText = sqlQueryInit;
                        command.ExecuteNonQuery();
                        

                    }


                    string sqlQueryAdd = "";


                    switch (tableName)
                    {
                        case "Products"://#####
                            sqlQueryAdd = $"INSERT INTO {tableName} VALUES(@name,@price,@collection,@description,@stock,@ageLimit,@discount,0,@DateReliesed)";
                            command.Parameters.AddWithValue("@name", item.ProductName);
                            command.Parameters.AddWithValue("@price", item.Price);
                            command.Parameters.AddWithValue("collection", item.Collection);
                            command.Parameters.AddWithValue("@description", item.Description);
                            command.Parameters.AddWithValue("@stock", item.Stock);
                            command.Parameters.AddWithValue("@ageLimit", item.AgeLimit);
                            command.Parameters.AddWithValue("@discount", item.Discount);
                            command.Parameters.AddWithValue("@DateReliesed", item.DateReliesed);




                            break;
                        case "Users":
                            sqlQueryAdd = $"INSERT INTO {tableName} VALUES(@UserName,@Password)";
                            command.Parameters.AddWithValue("@UserName", item.UserName);
                            command.Parameters.AddWithValue("@Password", item.Password);
                            break;
                        case "ShopList":
                            sqlQueryAdd = $"INSERT INTO {tableName} VALUES(@UserId,@ProductId)";
                            //sqlQueryAdd = @"
                            //MERGE INTO " + tableName + @" WITH (HOLDLOCK) AS target
                            //USING (VALUES (@UserId, @ProductId, @units)) 
                            //      AS source (UserId, ProductId, Units)
                            //      ON target.UserId = source.UserId AND target.ProductId = source.ProductId
                            //WHEN MATCHED THEN 
                            //    UPDATE SET Units = target.Units + source.Units
                            //WHEN NOT MATCHED THEN 
                            //    INSERT (UserId, ProductId, Units) VALUES (source.UserId, source.ProductId, source.Units);";

                            command.Parameters.AddWithValue("@UserId", item.UserId);
                            command.Parameters.AddWithValue("@ProductId", item.ProductId);
                           
                            break;

                    }
                    command.CommandText = sqlQueryAdd;
                    rowsAffected = command.ExecuteNonQuery();
                    //if (tableName =="Products")
                    //{
                    //    sqlQueryAdd = $"INSERT INTO Images VALUES(@ImagePath,@ProductId)";
                    //    foreach (string imagePath in item.PicturesPaths)
                    //    {
                    //        command.Parameters.AddWithValue("@ImagePath", imagePath);
                    //        command.Parameters.AddWithValue("@ProductId", item.ProductId);
                    //        command.CommandText = sqlQueryAdd;
                    //        command.ExecuteNonQuery();
                    //    }
                    //}
                }
            }



            return rowsAffected != 0;
        }
        public bool UpdateItemFrom(dynamic item, string tableName)
        {
            int rowsAffected = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sqlQueryUpdate = "";
                switch (tableName)
                {
                    case "Products":
                        sqlQueryUpdate = @"
                    UPDATE Products 
                    SET 
                        ProductName = @name,
                        Price = @price,
                        Collection = @collection,
                        Description = @description,
                        Stock = @stock,
                        AgeLimit = @ageLimit,
                        Discount = @discount,
                        DateReliesed = @DateReliesed
                    WHERE ProductId = @id";

                        using (SqlCommand command = new SqlCommand(sqlQueryUpdate, connection))
                        {
                            command.Parameters.AddWithValue("@id", item.ProductId);
                            command.Parameters.AddWithValue("@name", item.ProductName);
                            command.Parameters.AddWithValue("@price", item.Price);
                            command.Parameters.AddWithValue("collection", item.Collection);
                            command.Parameters.AddWithValue("@description", item.Description);
                            command.Parameters.AddWithValue("@stock", item.Stock);
                            command.Parameters.AddWithValue("@ageLimit", item.AgeLimit);
                            command.Parameters.AddWithValue("@discount", item.Discount);
                            command.Parameters.AddWithValue("@DateReliesed", item.DateReliesed);

                            rowsAffected = command.ExecuteNonQuery();
                        }
                        break;
                    case "Users":
                        sqlQueryUpdate = @"
                    UPDATE Users 
                    SET 
                        
                        UserName = @UserName,
                        Password = @Password
                    WHERE Id = @id";

                        using (SqlCommand command = new SqlCommand(sqlQueryUpdate, connection))
                        {
                            
                            command.Parameters.AddWithValue("@UserName", item.UserName);
                            command.Parameters.AddWithValue("@Password", item.Password);

                            rowsAffected = command.ExecuteNonQuery();
                        }
                        break;
                    case "ShopList":
                        sqlQueryUpdate = @"
                    UPDATE ShopList 
                    SET 
                        Id=@id,
                        UserId = @UserId,
                        ProductId = @ProductId
                    WHERE UserId = @userId AND ProductId = @productId";

                        using (SqlCommand command = new SqlCommand(sqlQueryUpdate, connection))
                        {
                            command.Parameters.AddWithValue("@id", item.Id);
                            command.Parameters.AddWithValue("@userId", item.UserId);
                            command.Parameters.AddWithValue("@ProductId", item.ProductId);
                            rowsAffected = command.ExecuteNonQuery();
                        }
                        break;
                }
            }

            return rowsAffected > 0;
        }
        public dynamic GetItemById(string tableName, int itemId)
        {
            dynamic item = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuerySelect = "";
                switch (tableName)
                {
                    case "Products":
                        sqlQuerySelect = "SELECT * FROM Products WHERE ProductId = @id";
                        break;
                    case "Users":
                        sqlQuerySelect = "SELECT * FROM Users WHERE Id = @id";
                        break;
                    case "ShopList":
                        sqlQuerySelect = "SELECT * FROM ShopList WHERE Id = @id";
                        break;
                        
                }

                using (SqlCommand command = new SqlCommand(sqlQuerySelect, connection))
                {
                    command.Parameters.AddWithValue("@id", itemId);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        switch (tableName)
                        {
                            case "Products":
                                item = new ProductsModel
                                {
                                    ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                    ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                                    Price = reader.GetInt32(reader.GetOrdinal("Price")),
                                    Collection = reader.GetString(reader.GetOrdinal("Collection")),
                                    Description = reader.GetString(reader.GetOrdinal("Description")),
                                    Stock = reader.GetInt32(reader.GetOrdinal("Stock")),
                                    AgeLimit = reader.GetInt32(reader.GetOrdinal("AgeLimit")),
                                    Discount = reader.GetInt32(reader.GetOrdinal("discount")),
                                    NumOfOrders = reader.GetInt32(reader.GetOrdinal("NumOfOrders")),
                                    DateReliesed = reader.GetDateTime(reader.GetOrdinal("DateReliesed"))

                                };
                                break;
                            case "Users":
                                item = new
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    UserName = reader.GetString(reader.GetOrdinal("UserName")),
                                    Password = reader.GetString(reader.GetOrdinal("Password")),
                                    
                                };
                                break;
                            case "ShopList":
                                item = new
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    UserId = reader.GetString(reader.GetOrdinal("UserId")),
                                    ProductId = reader.GetString(reader.GetOrdinal("ProductId")),
                                    
                                };
                                break;
                                
                        }
                    }
                    reader.Close();
                }
            }

            return item;
        }

        public CartModel createCart(string userId)
        {
            CartModel cart = new CartModel();
            cart.UserId = userId;
            List<string> userProductsId = GetListOf("ShopList").Where(p => p.userId == userId).Select(p => p.ProductId).Cast<string>().ToList();
            cart.products = GetListOf("products").Cast<ProductsModel>().ToList().Where(p => userProductsId.Contains(p.ProductId.ToString())).Cast<ProductsModel>().ToList();
            cart.TotalAmount = cart.products.Sum(p => p.Price);
            return cart;
        }

        public bool deleteFrom(int itemId,int userId, string tableName)
        {
            int rowsAffected = 0;
            string sqlQuery = null;
            
            using (SqlConnection connection = new SqlConnection(connectionString))
            {

                switch (tableName.ToLower())
            {
                case "shoplist":
                        sqlQuery = $"DELETE FROM {tableName} WHERE Id = (SELECT TOP 1 Id FROM {tableName} WHERE UserId=@UserId AND ProductId=@ProductId)";
                        break;
            }
            
                connection.Open();
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.CommandText = sqlQuery;
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@ProductId", itemId);
                    rowsAffected = command.ExecuteNonQuery();

                }
            }
            return rowsAffected > 0;
        }
        public bool updateFrom(int itemId, int userId, string tableName)
        {
            int rowsAffected = 0;
            string sqlQuery = null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {

                switch (tableName.ToLower())
                {
                    case "shoplist":
                        sqlQuery = $"DELETE FROM {tableName} WHERE UserId={userId} and ProductId={itemId}";
                        break;
                }

                connection.Open();
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.CommandText = sqlQuery;
                    rowsAffected = command.ExecuteNonQuery();

                }
            }
            return rowsAffected > 0;
        }

    }

}

