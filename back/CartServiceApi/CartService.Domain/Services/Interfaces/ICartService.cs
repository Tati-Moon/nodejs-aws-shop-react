using CartService.Domain.Entity;
using CartService.Domain.Models;

namespace CartService.Domain.Services.Interfaces
{
    public interface ICartService
    {
        Task<Cart> FindOrCreateByUserIdAsync(Guid userId);
        Task<Cart> FindByUserIdAsync(Guid userId);
        Task<Cart> UpdateByUserIdAsync(Guid userId, UpdateCartDto updateCartDto);
        Task RemoveByUserIdAsync(Guid userId);
    }
}
