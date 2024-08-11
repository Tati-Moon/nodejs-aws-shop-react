using CartService.Domain.Entity;
using CartService.Domain.Models;
using CartService.Domain.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CartService.Domain.Services
{
    public class OrderService(DatabaseContext context) : IOrderService
    {
        private readonly DatabaseContext _context = context;

        public async Task<Order> CreateOrderAsync(CreateOrderDto checkoutDto)
        {
            // 1. Валидация данных
            if (checkoutDto == null)
            {
                throw new ArgumentNullException(nameof(checkoutDto));
            }

            // 2. Поиск корзины по идентификатору
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == checkoutDto.CartId);

            if (cart == null)
            {
                throw new InvalidOperationException("Cart not found.");
            }

            // 3. Создание нового заказа
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = checkoutDto.UserId,
                CartId = checkoutDto.CartId ?? Guid.Empty,
                Payment = checkoutDto.Payment,
                Delivery = checkoutDto.Delivery,
                Comments = checkoutDto.Comments,
                Status = OrderStatus.Open,
                Total = checkoutDto.Total,
                //  CreatedAt = DateTime.UtcNow,
                //   UpdatedAt = DateTime.UtcNow
            };

            // 4. Добавление заказа в контекст и сохранение изменений
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return order;
        }

        public async Task<Order> FindByIdAsync(Guid orderId)
        {
            return await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task UpdateAsync(Guid orderId, UpdateOrderDto updateOrderDto)
        {
            var order = await FindByIdAsync(orderId) ?? throw new InvalidOperationException("Order not found");
            order.Payment = updateOrderDto.Payment ?? order.Payment;
            order.Delivery = updateOrderDto.Delivery ?? order.Delivery;
            order.Comments = updateOrderDto.Comments ?? order.Comments;
            order.Status = updateOrderDto.Status ?? order.Status;
            order.Total = updateOrderDto.Total ?? order.Total;

            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(Guid orderId)
        {
            var order = await FindByIdAsync(orderId);

            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
        }
    }
}