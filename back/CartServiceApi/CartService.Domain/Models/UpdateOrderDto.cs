using CartService.Domain.Entity;

namespace CartService.Domain.Models
{
    public class UpdateOrderDto
    {
        public string Payment { get; set; }
        public string Delivery { get; set; }
        public string Comments { get; set; }
        public OrderStatus? Status { get; set; }
        public decimal? Total { get; set; }
    }
}
