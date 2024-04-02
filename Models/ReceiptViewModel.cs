using System.Collections.Generic;

namespace ShopProject.Models
{
    public class ReceiptViewModel
    {
        public AccountModel CurrentAccount { get; set; }
        public List<ProductsModel> Products { get; set; }
    }
}
