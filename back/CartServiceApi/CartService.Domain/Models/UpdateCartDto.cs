using System.ComponentModel.DataAnnotations;

namespace CartService.Domain.Models
{
    public class UpdateCartDto
    {
       public List<CartItemDto> Items { get; set; }
    }

    public class CartItemDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public int Count { get; set; }

        [Required]
        public int Price { get; set; }
    }
}
