using System.ComponentModel.DataAnnotations.Schema;

namespace CartService.Domain.Entity
{
    public class Order
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("cart_id")]
        public Guid CartId { get; set; }

        [Column("payment")]
        public string Payment { get; set; }

        [Column("delivery")]
        public string Delivery { get; set; }

        [Column("comments")]
        public string Comments { get; set; }

        [Column("status")]
        public OrderStatus Status { get; set; }

        [Column("total")]
        public decimal Total { get; set; }

        public User User { get; set; }

        public Cart Cart { get; set; }
    }
}
