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

        public IActionResult AddToCart(int productId, string productList)
        {

            string userJson = HttpContext.Session.GetString("CurrentAccount");
            AccountModel currentAccount = null;

            if (!string.IsNullOrEmpty(userJson))
            {
                currentAccount = JsonConvert.DeserializeObject<AccountModel>(userJson);
            }
            shop.AddItemTo(new { UserId = currentAccount.UserID, ProductId = productId }, "ShopList");
            ProductsModel product = (ProductsModel)shop.GetItemById("Products", productId);
            product.Stock--;
            shop.UpdateItemFrom(product, "Products");
            currentCollection = HttpContext.Session.GetString("CurrentCollection");
            if (currentCollection != null)
            {
                return RedirectToAction("ProductsCollection", new { collection = currentCollection });
            }
            return View("MyProducts", list);
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
            if (product != null)
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

            return View(shop.GetListOf("Products")
    .Cast<ProductsModel>().ToList());


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
        [HttpPost]
        public IActionResult FilteredProducts(string searchedInput)
        {
            List<ProductsModel> listTemp = list;
            if (!string.IsNullOrEmpty(searchedInput))
            {
                listTemp = list.Where(p => p.ProductName.Contains(searchedInput, StringComparison.OrdinalIgnoreCase) ||
                p.Description.Contains(searchedInput, StringComparison.OrdinalIgnoreCase)).ToList();

            }
            return View("MyProducts", listTemp);
        }
        public IActionResult ProductDetails(int id)
        {

            ProductsModel product = list.FirstOrDefault(p => (p.ProductId) == id);
            return View(product);
        }
        public IActionResult CheckOut(string FirstName,string LastName, string Age, string Address, string Email, string Phone)
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
            return View("Payment");
        }
        public IActionResult BeforePayment()
        {
            return View("BeforePayment");
        }
        public IActionResult ToPayment(string CardHolderName, string CreditNumber, string CreditCVC, string ExpiryDateMonth, string ExpiryDateYear)
        {

            string userJson = HttpContext.Session.GetString("CurrentAccount");
            AccountModel currentAccount = null;

            if (!string.IsNullOrEmpty(userJson))
            {
                currentAccount = JsonConvert.DeserializeObject<AccountModel>(userJson);
            }
            var encryptionKey = KeyGenerator.GenerateRandomKey(16);
            var IV= KeyGenerator.GenerateRandomIV(16);
            try
            {
                // Encrypt the credit card number
                string encryptedCardNumber = EncryptString(CardHolderName, encryptionKey, IV);
                string encryptedCreditCVC = EncryptString(CreditCVC, encryptionKey, IV);
                string encryptedExpiryDate = EncryptString(ExpiryDateMonth+"/"+ExpiryDateYear, encryptionKey, IV);
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
            return View("Payment");
        }

        public IActionResult ProductsCollection(string collection)
        {
            HttpContext.Session.SetString("CurrentCollection", collection);
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
