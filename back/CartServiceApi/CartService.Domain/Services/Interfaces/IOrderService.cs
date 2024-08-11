using CartService.Domain.Entity;
using CartService.Domain.Models;

namespace CartService.Domain.Services.Interfaces
{
    public interface IOrderService
    {
        Task<Order> FindByIdAsync(Guid orderId);
        Task UpdateAsync(Guid orderId, UpdateOrderDto updateOrderDto);
        Task RemoveAsync(Guid orderId);
        Task<Order> CreateOrderAsync(CreateOrderDto checkoutDto);
    }
}
