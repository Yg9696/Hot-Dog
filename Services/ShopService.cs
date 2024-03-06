using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using ShopProject.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

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
                    case "shopList":
                        tableName = "ShopList";
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
                                case "ShopList":
                                    item = new
                                    {
                                        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                        ProductId = reader.GetString(reader.GetOrdinal("ProductId"))
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
                                sqlQueryInit = $"CREATE TABLE {tableName}(Id INT PRIMARY KEY IDENTITYUserName VARCHAR(30) PRIMARY KEY IDENTITY, Password VARCHAR(30))";
                                break;
                            case "ShopList":
                                sqlQueryInit = $"CREATE TABLE {tableName}(UserId VARCHAR(30) , ProductId VARCHAR(30))";
                                break;

                        }

                        command.CommandText = sqlQueryInit;
                        command.ExecuteNonQuery();
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
                            command.Parameters.AddWithValue("@UserId", item.UserId);
                            command.Parameters.AddWithValue("@ProductId", item.ProductId);
                            break;

                    }
                    command.CommandText = sqlQueryAdd;
                    rowsAffected = command.ExecuteNonQuery();
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
    }

}
//public class ApplicationUser
//{
//    public string UserId { get; set; }
//    public string UserName { get; set; }
//}
//public class UserMiddleware
//{
//    private readonly RequestDelegate _next;

//    public UserMiddleware(RequestDelegate next)
//    {
//        _next = next;
//    }

//    public async Task Invoke(HttpContext httpContext)
//    {
//        // Fetch user information from wherever it's stored
//        // For example, from session, JWT token, database, etc.
//        ApplicationUser user = GetUserFromSomeWhere();

//        // Add the user object to the HttpContext
//        httpContext.Items["CurrentUser"] = user;

//        await _next(httpContext);
//    }
//}





