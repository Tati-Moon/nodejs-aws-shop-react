namespace CartService.Domain.Models
{
    public class CheckoutDto
    {
        public Guid CartId { get; set; }
        public string Payment { get; set; }
        public string Delivery { get; set; } 
        public string Comments { get; set; }
        public decimal Total { get; set; }
    }
}
