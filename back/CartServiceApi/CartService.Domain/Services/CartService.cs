using CartService.Domain.Entity;
using CartService.Domain.Models;
using CartService.Domain.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CartService.Domain.Services
{
    public class CartService(DatabaseContext context) : ICartService
    {
        private readonly DatabaseContext _context = context;

        public async Task<Cart> FindOrCreateByUserIdAsync(Guid userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Status = CartStatus.Open
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task<Cart> FindByUserIdAsync(Guid userId)
        {
            return await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<Cart> UpdateByUserIdAsync(Guid userId, UpdateCartDto updateCartDto)
        {
            var cart = await FindOrCreateByUserIdAsync(userId);

            if (updateCartDto.Items != null)
            {
                foreach (var itemDto in updateCartDto.Items)
                {
                    var item = cart.CartItems
                        .FirstOrDefault(ci => ci.ProductId == itemDto.ProductId);

                    if (item != null)
                    {
                        item.Count = itemDto.Count;
                        item.Price = itemDto.Price;
                    }
                    else
                    {
                        cart.CartItems.Add(new CartItem
                        {
                            CartId = cart.Id,
                            ProductId = itemDto.ProductId,
                            Count = itemDto.Count,
                            Price = itemDto.Price
                        });
                    }
                }

                // Remove items that are no longer in the cart
                var itemIds = updateCartDto.Items.Select(i => i.ProductId).ToHashSet();
                cart.CartItems = cart.CartItems.Where(ci => itemIds.Contains(ci.ProductId)).ToList();
            }

            cart.UpdatedAt = DateTime.UtcNow;
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();

            return cart;
        }

        public async Task RemoveByUserIdAsync(Guid userId)
        {
            var cart = await FindByUserIdAsync(userId);

            if (cart != null)
            {
                _context.Carts.Remove(cart);
                await _context.SaveChangesAsync();
            }
        }
    }
}
