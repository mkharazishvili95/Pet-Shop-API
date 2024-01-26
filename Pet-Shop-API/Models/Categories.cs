using System.ComponentModel.DataAnnotations;

namespace Pet_Shop_API.Models
{
    public class Categories
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
