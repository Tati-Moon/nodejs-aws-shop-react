using CartService.Domain.Entity;
using CartServiceApi.Model;

namespace CartServiceApi.Mapper
{
    public static class CartMapper
    {
        public static CartModel MapToCartModel(Cart cart)
        {
            if (cart == null) return null;

            return new CartModel
            {
                Id = cart.Id,
                UserId = cart.UserId,
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt,
                Status = cart.Status,
                CartItems = cart.CartItems.Select(ci => new CartItemModel
                {
                    CartId = ci.CartId,
                    ProductId = ci.ProductId,
                    Count = ci.Count,
                    Price = ci.Price
                }).ToList()
            };
        }
    }
}