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
        //public Picture[] Pictures { get; set; }
        public string Description { get; set; } 
        public int Stock { get; set; }  



    }

}
public class Picture
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Url { get; set; }
    public string ImageMimeType { get; set; }
}

