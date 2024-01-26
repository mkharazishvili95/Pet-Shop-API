using System.ComponentModel.DataAnnotations;

namespace Pet_Shop_API.Models
{
    public class Bank
    {
        [Key]
        public int Id { get; set; }
        public double Balance {  get; set; }
    }
}
