using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ShopProject.Models;
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
                                        Stock = reader.GetInt32(reader.GetOrdinal("Stock"))
                                    };
                                    break;
                                case "shoplist":
                                    item = new
                                    {
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
                                sqlQueryInit = $"CREATE TABLE {tableName}(ProductId INT PRIMARY KEY IDENTITY, ProductName VARCHAR(30), Price INT, Collection VARCHAR(100), Description VARCHAR(255), Stock INT)";
                                break;
                            case "Users":
                                sqlQueryInit = $"CREATE TABLE {tableName}(Id INT PRIMARY KEY IDENTITY,UserName VARCHAR(30) PRIMARY KEY, Password VARCHAR(30))";
                                break;
                            case "ShopList":
                                sqlQueryInit = $"CREATE TABLE {tableName} (UserId VARCHAR(30), ProductId VARCHAR(30))"; 
                                break;

                        }

                        command.CommandText = sqlQueryInit;
                        command.ExecuteNonQuery();
                        if (tableName == "Products")
                        {
                            command.CommandText = $"SELECT CASE WHEN EXISTS (SELECT * FROM sys.tables WHERE name = 'Images' AND schema_id = SCHEMA_ID('dbo')) THEN 1 ELSE 0 END";
                            if ((int)command.ExecuteScalar() == 0)
                            {
                                command.CommandText = $"CREATE TABLE Images(ImageId INT PRIMARY KEY IDENTITY, ImagePath VARCHAR(30),ProductId INT , FOREIGN KEY (ProductId) REFERENCES Products(ProductId))";
                                command.ExecuteNonQuery();
                            }
                        }

                    }


                    string sqlQueryAdd = "";


                    switch (tableName)
                    {
                        case "Products"://#####
                            sqlQueryAdd = $"INSERT INTO {tableName} VALUES(@name,@price,@collection,@description,@stock)";
                            command.Parameters.AddWithValue("@name", item.ProductName);
                            command.Parameters.AddWithValue("@price", item.Price);
                            command.Parameters.AddWithValue("collection", item.Collection);
                            command.Parameters.AddWithValue("@description", item.Description);
                            command.Parameters.AddWithValue("@stock", item.Stock);


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

