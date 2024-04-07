using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ShopProject.Models;
using ShopProject.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ShopProject.Controllers
{

    public class ProductsController : Controller
    {
        public bool sign;
        private readonly IConfiguration _configuration;
        private readonly string connectionString;
        private readonly ShopService shop;
        private List<ProductsModel> list;
        private AccountModel currentAccount;
        public string currentCollection;
        private readonly IHttpContextAccessor _httpContextAccessor;
        


        public ProductsController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            sign = true;
            _configuration = configuration;
            connectionString = configuration.GetConnectionString("myConnect");
            shop = new ShopService(_configuration);
            string userJson = _httpContextAccessor.HttpContext.Session.GetString("CurrentAccount");
            currentAccount = null;
           
            if (!string.IsNullOrEmpty(userJson))
            {
                currentAccount = JsonConvert.DeserializeObject<AccountModel>(userJson);
            }
            if (currentAccount.Age != "")
            {
                list = shop.GetListOf("Products").Cast<ProductsModel>().ToList().Where(p => p.AgeLimit <= Convert.ToInt32(currentAccount.Age)).ToList();
            }
            else
            {
                list = shop.GetListOf("Products").Cast<ProductsModel>().ToList();
            }
        }

        public IActionResult Receipt()
        {
            return View("Receipt");
        }

        public IActionResult addProducts()
        {
            return View("addProduct");
        }

        public IActionResult AllProducts()
        {
            return View("MyProducts");
        }
        public IActionResult Item()
        {
            return View("item");
        }

        public IActionResult Cart()
        {
            string userJson = HttpContext.Session.GetString("CurrentAccount");
            AccountModel currentAccount = null;

            if (!string.IsNullOrEmpty(userJson))
            {
                currentAccount = JsonConvert.DeserializeObject<AccountModel>(userJson);
            }
            List<ProductsModel> listTemp = new List<ProductsModel>();


            if (currentAccount != null)
            {
                var shopListProductIds = shop.GetListOf("ShopList")
                             .Where(p => int.Parse(p.UserId) == currentAccount.UserID)
                             .Select(p => int.Parse(p.ProductId))
                             .ToList();


                foreach (dynamic p in shopListProductIds)
                {

                    listTemp.Add(list.Find(product => product.ProductId == p));
                    if (listTemp[0] == null) { listTemp = null; }
                }

            }
            return View("cart", listTemp);
        }


        public IActionResult AddToCart(int productId)
        {
            string userJson = HttpContext.Session.GetString("CurrentAccount");
            AccountModel currentAccount = null;

            if (!string.IsNullOrEmpty(userJson))
            {
                currentAccount = JsonConvert.DeserializeObject<AccountModel>(userJson);
            }

            // Add item to user's shopping cart
            shop.AddItemTo(new { UserId = currentAccount.UserID, ProductId = productId }, "ShopList");

            // Retrieve product from shop by productId
            ProductsModel product = (ProductsModel)shop.GetItemById("Products", productId);

            // Update stock immediately
            if (product.Stock > 0)
            {
                product.Stock--;
                shop.UpdateItemFrom(product, "Products");
            }

            // Return JSON response with updated stock value
            return Json(new { updatedStock = product.Stock });
        }

        public IActionResult AddProduct()
        {
            ;
            ProductsModel product = new ProductsModel();
            return View(product);
        }
        [HttpPost]
        public IActionResult MyProducts(ProductsModel product)
        {
            product.DateReliesed = DateTime.Now.Date;
            if (product.ProductName != null)
            {
                shop.AddItemTo(product, "Products");
            }
            return RedirectToAction("MyProducts");
        }
        [HttpGet]

        public IActionResult MyProducts()
        {
            
            string userJson = HttpContext.Session.GetString("CurrentAccount");
            AccountModel currentAccount = null;

            if (!string.IsNullOrEmpty(userJson))
            {
                currentAccount = JsonConvert.DeserializeObject<AccountModel>(userJson);
            }
            
            if(currentAccount.UserName != "Admin") { return View(list); }
            else { return View("MyProductsAdmin", list); }
            


        }

        [HttpPost]
        public IActionResult SortProducts(string selectedFilter, string sign, int minPrice, int maxPrice)
        {
            currentCollection = HttpContext.Session.GetString("CurrentCollection");
            List<ProductsModel> listTemp = list;
            if (currentCollection != null)
            {

                listTemp = list.Where(p => p.Collection == currentCollection).ToList();
            }
            if (selectedFilter != null)
            {
                if (sign == "up")
                {

                    switch (selectedFilter)
                    {
                        case "name":
                            listTemp = listTemp.OrderBy(l => l.ProductName).ToList();
                            break;
                        case "price":
                            listTemp = listTemp.OrderBy(l => l.Price).ToList();
                            break;
                        case "collection":
                            listTemp = listTemp.OrderBy(l => l.Collection).ToList();
                            break;
                        case "stock":
                            listTemp = listTemp.OrderBy(l => l.Stock).ToList();
                            break;
                        case "Popularity":
                            listTemp = listTemp.OrderBy(l => l.NumOfOrders).ToList();
                            break;
                        case "DateModified":
                            listTemp = listTemp.OrderBy(l => l.DateReliesed.Ticks).ToList();
                            break;




                    }
                }
                else
                {
                    switch (selectedFilter)
                    {
                        case "name":
                            listTemp = listTemp.OrderByDescending(l => l.ProductName).ToList();
                            break;
                        case "price":
                            listTemp = listTemp.OrderByDescending(l => l.Price).ToList();
                            break;
                        case "collection":
                            listTemp = listTemp.OrderByDescending(l => l.Collection).ToList();
                            break;
                        case "stock":
                            listTemp = listTemp.OrderByDescending(l => l.Stock).ToList();
                            break;
                        case "Popularity":
                            listTemp = listTemp.OrderByDescending(l => l.NumOfOrders).ToList();
                            break;
                        case "DateModified"://dosent work
                            listTemp = listTemp.OrderByDescending(l => l.DateReliesed.Ticks).ToList();
                            break;

                    }
                }
            }
            if (sign == "Search")
            {
                if (minPrice != 0 || maxPrice > 0)
                {
                    if (minPrice > maxPrice)
                    {
                        TempData["AlertMessage"] = "Input error -min price can't be higher then max price";
                    }
                    else
                    {

                        listTemp = list.Where(l =>
                        (l.Price * (Convert.ToSingle(100 - l.Discount) / 100)) <= maxPrice &&
                        (l.Price * (Convert.ToSingle(100 - l.Discount) / 100)) >= minPrice
                    ).ToList();

                    }
                }
            }

            return View("MyProducts", listTemp);
        }
        public  IActionResult NotifyMe(int productId)
        {
            string userJson = HttpContext.Session.GetString("CurrentAccount");
            AccountModel currentAccount = null;

            if (!string.IsNullOrEmpty(userJson))
            {
                currentAccount = JsonConvert.DeserializeObject<AccountModel>(userJson);
            }
            shop.AddItemTo(new { UserId = currentAccount.UserID, ProductId = productId }, "NotifyList");
            return Ok();
            
        }

            [HttpPost]
        public IActionResult FilteredProducts(string searchedInput)
        {
            List<ProductsModel> listTemp = list;
            if (!string.IsNullOrEmpty(searchedInput))
            {
                listTemp = list.Where(p => p.ProductName.Contains(searchedInput, StringComparison.OrdinalIgnoreCase) ||
                p.Description.Contains(searchedInput, StringComparison.OrdinalIgnoreCase) || searchedInput.Contains((p.ProductId).ToString(), StringComparison.OrdinalIgnoreCase)).ToList();

            }
            return View("MyProducts", listTemp);
        }
        public IActionResult ProductDetails(int id)
        {

            ProductsModel product = list.FirstOrDefault(p => (p.ProductId) == id);
            return View(product);
        }
        public IActionResult CheckOut(string id,string FirstName,string LastName, string Age, string Address, string Email, string Phone)
        {
            string userJson = HttpContext.Session.GetString("CurrentAccount");
            AccountModel currentAccount = null;

            if (!string.IsNullOrEmpty(userJson))
            {
                currentAccount = JsonConvert.DeserializeObject<AccountModel>(userJson);
            }
            currentAccount.FirstName= FirstName;
            currentAccount.LastName= LastName;
            currentAccount.Age= Age;
            currentAccount.FullAddress= Address;
            currentAccount.Email= Email;
            currentAccount.PhoneNumber= Phone;
            var jsonString = System.Text.Json.JsonSerializer.Serialize(currentAccount);
            HttpContext.Session.SetString("CurrentAccount", jsonString);
            List<ProductsModel> listTemp = new List<ProductsModel>();
            if (id == null)
            {
                if (currentAccount != null)
                {
                    var shopListProductIds = shop.GetListOf("ShopList")
                                 .Where(p => int.Parse(p.UserId) == currentAccount.UserID)
                                 .Select(p => int.Parse(p.ProductId))
                                 .ToList();


                    foreach (dynamic p in shopListProductIds)
                    {

                        listTemp.Add(list.Find(product => product.ProductId == p));
                        if (listTemp[0] == null) { listTemp = null; }
                    }

                }
            }
            else { listTemp.Add(shop.GetItemById("Products", int.Parse(id))); }
            return View("Payment", listTemp);
            
           
            
        }
        public IActionResult BeforePayment()
        {
            return View("BeforePayment");
        }
        public IActionResult ToPayment(string id,string CardHolderName, string CreditNumber, string CreditCVC, string ExpiryDateMonth, string ExpiryDateYear)
        {
            string userJson = HttpContext.Session.GetString("CurrentAccount");
            AccountModel currentAccount = null;

            if (!string.IsNullOrEmpty(userJson))
            {
                currentAccount = JsonConvert.DeserializeObject<AccountModel>(userJson);
            }
            
            var encryptionKey = KeyGenerator.GenerateRandomKey(16);
            var IV = KeyGenerator.GenerateRandomIV(16);
            try
            {
                // Encrypt the credit card number
                string encryptedCardNumber = EncryptString(CardHolderName, encryptionKey, IV);
                string encryptedCreditCVC = EncryptString(CreditCVC, encryptionKey, IV);
                string encryptedExpiryDate = EncryptString(ExpiryDateMonth + "/" + ExpiryDateYear, encryptionKey, IV);
                // Save the encrypted credit card number to SQL Server
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = $"UPDATE Accounts SET EncryptedCardNumber = CONVERT(VARBINARY(MAX), @EncryptedCardNumber), EncryptedExpiryDate = CONVERT(VARBINARY(MAX), @EncryptedExpiryDate), EncryptedCVV = CONVERT(VARBINARY(MAX), @EncryptedCVV), IV = CONVERT(VARBINARY(MAX), @IV), KeyIdentifier = @KeyIdentifier WHERE Id = {currentAccount.UserID}";
                    SqlCommand command = new SqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@EncryptedCardNumber", encryptedCardNumber);
                    command.Parameters.AddWithValue("@EncryptedExpiryDate", encryptedExpiryDate);
                    command.Parameters.AddWithValue("@EncryptedCVV", encryptedCreditCVC);
                    command.Parameters.AddWithValue("@IV", IV);
                    command.Parameters.AddWithValue("@KeyIdentifier", encryptionKey);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving encrypted credit card: " + ex.Message);
            }
            return RedirectToAction("Recipt","Products",id);
        }
        public IActionResult Recipt(string id)
        {
            string userJson = HttpContext.Session.GetString("CurrentAccount");
            AccountModel currentAccount = null;

            if (!string.IsNullOrEmpty(userJson))
            {
                currentAccount = JsonConvert.DeserializeObject<AccountModel>(userJson);
            }
            
            List<ProductsModel> listTemp = new List<ProductsModel>();
            ReceiptViewModel receipt = new ReceiptViewModel();
            
            if (currentAccount != null)
            {
                if (id != null)
                {
                    var shopListProductIds = shop.GetListOf("ShopList")
                                 .Where(p => int.Parse(p.UserId) == currentAccount.UserID)
                                 .Select(p => int.Parse(p.ProductId))
                                 .ToList();


                    foreach (dynamic p in shopListProductIds)
                    {

                        listTemp.Add(list.Find(product => product.ProductId == p));
                        if (listTemp[0] == null) { listTemp = null; }
                    }
                }
                else { listTemp.Add(shop.GetItemById("Products", int.Parse(id))); }

            }
            if (id != null)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = $"DELETE FROM ShopList WHERE UserId = {currentAccount.UserID}";
                    SqlCommand command = new SqlCommand(sql, connection);
                    command.ExecuteNonQuery();
                    connection.Close();
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
            }
            receipt.Products = listTemp;
            receipt.CurrentAccount= currentAccount;
            return View("Receipt", receipt);
           
        }
        public IActionResult Edit(int id)
        {
            return View("Edit",id);
        }

            public IActionResult EditProducts(string id,string ChangePrice,string ChangeStock)
            {
            ProductsModel product = shop.GetItemById("Products", int.Parse(id));
            if (product != null) {
                if (ChangePrice != null) {
                    product.Price = int.Parse(ChangePrice);
                }
                if (ChangeStock != null) { 
                    product.Stock = int.Parse(ChangeStock);
            }
                shop.UpdateItemFrom(product, "Products");
            }
            return View("Edit", int.Parse(id));
        }

        public IActionResult BuyNow(string id)
        {
            return View("BeforePayment", id);
        }
        

            public IActionResult ProductsCollection(string collection)
        {

            if (collection != null)
            {
                HttpContext.Session.SetString("CurrentCollection", collection);
            }
            else
            {
                collection = HttpContext.Session.GetString("CurrentCollection");
            }
            return View("MyProducts", list.Where(p => p.Collection == collection).ToList());
        }
        public IActionResult deleteFromCart(int itemId)
        {
            string userJson = HttpContext.Session.GetString("CurrentAccount");
            AccountModel currentAccount = null;

            if (!string.IsNullOrEmpty(userJson))
            {
                currentAccount = JsonConvert.DeserializeObject<AccountModel>(userJson);
            }
            shop.deleteFrom(itemId, currentAccount.UserID, "ShopList");
            ProductsModel product = shop.GetItemById("Products", itemId);
            product.Stock++;
            shop.UpdateItemFrom(product, "Products");
            return RedirectToAction("Cart");
        }

        public string EncryptString(string plainText, byte[] key, byte[] iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                // Create an encryptor to perform the stream transform
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            // Write all data to the stream
                            swEncrypt.Write(plainText);
                        }
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
        }
}
    public class KeyGenerator
    {
        public static byte[] GenerateRandomKey(int keySizeInBytes)
        {
            byte[] key = new byte[keySizeInBytes];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(key);
            }
            return key;
        }

        public static byte[] GenerateRandomIV(int ivSizeInBytes)
        {
            byte[] iv = new byte[ivSizeInBytes];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(iv);
            }
            return iv;
        }
    }
}
