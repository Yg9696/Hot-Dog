using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace ShopProject.Models
{
    public class UserRegister
    {
       // [Key]
        public int UserID { get; set; }
        //[Required(ErrorMessage ="First Name is required.")]
        public string FirstName { get; set; }
        //[Required(ErrorMessage = "Last Name is required.")]
        public string LastName { get; set; }
        //[Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
    }
}
