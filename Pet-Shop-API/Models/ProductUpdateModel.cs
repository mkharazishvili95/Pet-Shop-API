namespace Pet_Shop_API.Models
{
    public class ProductUpdateModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price {  get; set; }
        public int Quantity {  get; set; }
        public bool IsAvailable {  get; set; }
        public int CategoryId {  get; set; }
    }
}
