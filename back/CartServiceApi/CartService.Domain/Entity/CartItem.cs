using System.ComponentModel.DataAnnotations.Schema;

namespace CartService.Domain.Entity
{
    public class CartItem
    {
        [Column("cart_id")]
        public Guid CartId { get; set; }

        [Column("product_id")]
        public Guid ProductId { get; set; }

        [Column("count")]
        public int Count { get; set; }

        [Column("price")]
        public int Price { get; set; }

        public Cart Cart { get; set; }
    }
}
