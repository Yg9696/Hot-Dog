using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace ShopProject.Models
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Price { get; set; }
        public string Collection { get; set; }

        public string Description { get; set; }
        public int Stock { get; set; }
        public IEnumerable<IFormFile> Images { get; set; }
    }
}
