namespace Pet_Shop_API.Models
{
    public class PurchaseModel
    {
        public int Id {  get; set; }
        public int UserId { get; set; }
        public DateTime BuyingDate { get; set; } = DateTime.UtcNow;
        public int  ProductId { get; set; }
        public double TotalPayment { get; set; }
        public int Quantity { get; set; }
    }
}
