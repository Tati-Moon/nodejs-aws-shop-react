using System.ComponentModel.DataAnnotations.Schema;

namespace CartService.Domain.Entity
{
    public class Cart
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [Column("status")]
        public CartStatus Status { get; set; }

        public User User { get; set; }

        public ICollection<CartItem> CartItems { get; set; }
    }
}
