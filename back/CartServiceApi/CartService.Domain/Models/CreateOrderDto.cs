using CartService.Domain.Entity;

namespace CartService.Domain.Models
{
    public class CreateOrderDto
    {
        public Guid UserId { get; set; }
        public Guid? CartId { get; set; }
        public string Payment { get; set; }
        public string Delivery { get; set; }
        public string Comments { get; set; }
        public OrderStatus Status { get; set; }
        public decimal Total { get; set; }

    }
}
