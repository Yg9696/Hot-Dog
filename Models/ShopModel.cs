using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;

namespace ShopProject.Models
{
    public class ShopModel
    {
        [Key]
        public string Name { get; set; }
        public ShopModel (string name) {
            Name = name;
        }
    }
}
