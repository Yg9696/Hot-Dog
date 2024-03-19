using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
        public List<string> PicturesPaths { get; set; }
        public int units { get; set; }



    }

}


