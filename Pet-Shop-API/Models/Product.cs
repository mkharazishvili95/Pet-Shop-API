using System.ComponentModel.DataAnnotations.Schema;

namespace Pet_Shop_API.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Quantity { get; set; }
        public bool IsAvailable {  get; set; }
        [ForeignKey("Categories")]
        public int CategoryId {  get; set; }
        public Categories Categories { get; set; }
    }
}
