using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using ShopProject.Models;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace ShopProject.Services
{
    public interface IShopService
    {

        //List<ProductsModel> GetProducts();
        public List<dynamic> GetListOf(string listType);
        //UsersModel[] GetUsers();
        //bool AddProduct(ProductsModel product);
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
        //public List<ProductsModel> GetProducts()
        //{
        //    using (SqlConnection connection = new SqlConnection(connectionString))
        //    {
        //        connection.Open();
        //        string sqlQuery = "SELECT CASE WHEN EXISTS (SELECT * FROM sys.tables WHERE name = 'Products' AND schema_id = SCHEMA_ID('dbo')) THEN 1 ELSE 0 END";
        //        using (SqlCommand command = new SqlCommand(sqlQuery, connection))
        //        {
        //            if ((int)command.ExecuteScalar() == 0)
        //            {
        //                return new List<ProductsModel>();
        //            }
        //        }

        //        string sqlProducts = "SELECT * FROM Products";
        //        using (SqlCommand command = new SqlCommand(sqlProducts, connection))
        //        {
        //            List<ProductsModel> products = new List<ProductsModel>();
        //            using (SqlDataReader reader = command.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    ProductsModel product = new ProductsModel
        //                    {
        //                        ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
        //                        ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
        //                        Price = reader.GetInt32(reader.GetOrdinal("Price")),
        //                        Collection = reader.GetString(reader.GetOrdinal("Collection")),
        //                        Description = reader.GetString(reader.GetOrdinal("Description")),
        //                        Stock = reader.GetInt32(reader.GetOrdinal("Stock"))
        //                    };
        //                    products.Add(product);

        //                }
        //            }
        //            return products;
        //        }

        //    }
        //}

        public List<dynamic> GetListOf(string listType)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string tableName = "";
                switch (listType.ToLower())
                {
                    case "products":
                        tableName = "Products";
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
                            switch (listType.ToLower())
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
                        switch (tableName)
                        {
                            case "Products":
                                sqlQueryInit= $"CREATE TABLE {tableName}(ProductId INT PRIMARY KEY IDENTITY, ProductName VARCHAR(30), Price INT, Collection VARCHAR(100), Description VARCHAR(255), Stock INT)";
                                break;
                            case "Users":
                                sqlQueryInit = $"CREATE TABLE {tableName}(UserName VARCHAR(30) PRIMARY KEY IDENTITY, Password VARCHAR(30))";
                                break;

                        }

                        command.CommandText = sqlQueryInit;
                        command.ExecuteNonQuery();
                    }

                    
                    string sqlQueryAdd = "";
                    

                    switch (tableName)
                    {
                        case "Products":
                            sqlQueryAdd = $"INSERT INTO {tableName} VALUES(@name,@price,@collection,@description,@stock)";
                            command.Parameters.AddWithValue("@name", item.ProductName);
                            command.Parameters.AddWithValue("@price", item.Price);
                            command.Parameters.AddWithValue("collection", item.Collection);
                            command.Parameters.AddWithValue("@description",item.Description);
                            command.Parameters.AddWithValue("@stock", item.Stock);
                            break;
                        case "Users":
                            sqlQueryAdd = $"INSERT INTO {tableName} VALUES(@UserName,@Password)";
                            command.Parameters.AddWithValue("@UserName", item.UserName);
                            command.Parameters.AddWithValue("@Password", item.Password);
                            break;
                    }
                    command.CommandText = sqlQueryAdd;
                    rowsAffected = command.ExecuteNonQuery();
                }
            }

            return rowsAffected != 0;
        }
        
        
    }
        //public UsersModel[] GetUsers()
        //{

        //}


    }

