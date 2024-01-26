using Pet_Shop_API.Identity;
using System.ComponentModel.DataAnnotations;

namespace Pet_Shop_API.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int Age { get; set; }
        public double Balance { get; set; } = 0;
        public int ContactNumber {  get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Role { get; set; } = Roles.User;
    }
}
