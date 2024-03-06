using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopProject.Models
{
    public class CartModel
    {
        public string UserId { get; set; }
        public List <ProductsModel> products { get; set; }
        public int TotalAmount { get; set; }
    }
}
