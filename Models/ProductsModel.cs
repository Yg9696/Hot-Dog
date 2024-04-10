using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ShopProject.Models
{
    public class ProductsModel
    {
        [Key]
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Price { get; set; }
        public string Collection { get; set; }
        
        public string Description { get; set; } 
        public int Stock { get; set; }
        public int AgeLimit { get; set; } = 0;
        public int Discount { get; set; } = 0;
        public int NumOfOrders { get; set; } = 0;
        public DateTime DateReliesed { get; set; }
        public int Units { get; set; }
        public string Image { get; set; }



    }

}


