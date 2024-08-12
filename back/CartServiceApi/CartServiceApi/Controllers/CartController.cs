using CartService.Domain.Entity;
using CartService.Domain.Models;
using CartService.Domain.Services.Interfaces;
using CartServiceApi.Mapper;
using CartServiceApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CartServiceApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/profile/cart")]
    public class CartController(
        ICartService cartService,
        IOrderService orderService,
        IUserService userService) : ControllerBase
    {
        private readonly ICartService _cartService = cartService;
        private readonly IOrderService _orderService = orderService;
        private readonly IUserService _userService = userService;

        private async Task<Guid> GetUserIdAsync()
        {
            return (await _userService.GetUserByNameAsync(User?.Identity?.Name?? "test3"))?.Id ?? Guid.Empty;
        }

        [HttpGet]
        public async Task<IActionResult> FindUserCart()
        {
            var userId = await GetUserIdAsync();
            var cart = await _cartService.FindOrCreateByUserIdAsync(userId);

            return Ok(new
            {
                statusCode = 200,
                message = "OK",
                data = new CarProfileModel
                {
                    Cart = CartMapper.MapToCartModel(cart),
                    Total = CalculateCartTotal(cart)
                }
            });
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateUserCart([FromBody] UpdateCartDto updateCartDto)
        {
            var userId = await GetUserIdAsync();
            var cart = await _cartService.UpdateByUserIdAsync(userId, updateCartDto);

            return Ok(new
            {
                statusCode = 200,
                message = "OK",
                data = new CarProfileModel
                {
                    Cart = CartMapper.MapToCartModel(cart),
                    Total = CalculateCartTotal(cart)
                }
            });
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> ClearUserCart()
        {
            var userId = await GetUserIdAsync();
            await _cartService.RemoveByUserIdAsync(userId);

            return Ok(new
            {
                statusCode = 200,
                message = "OK"
            });
        }

        [HttpPost("checkout")]
        [Authorize]
        public async Task<IActionResult> Checkout([FromBody] CheckoutDto checkoutDto)
        {
            var userId = await GetUserIdAsync();
            var cart = await _cartService.FindByUserIdAsync(userId);

            if (cart == null || cart.CartItems.Count == 0)
            {
                return BadRequest(new
                {
                    statusCode = 400,
                    message = "Cart is empty"
                });
            }

            var total = CalculateCartTotal(cart);
            var order = await _orderService.CreateOrderAsync(new CreateOrderDto
            {
                UserId = userId,
                CartId = cart.Id,
                Payment = checkoutDto.Payment,
                Delivery = checkoutDto.Delivery,
                Comments = checkoutDto.Comments,
                Total = total,
                Status = OrderStatus.Open
            });

            await _cartService.RemoveByUserIdAsync(userId);

            return Ok(new
            {
                statusCode = 200,
                message = "OK",
                data = new
                {
                    order
                }
            });
        }

        private static decimal CalculateCartTotal(Cart cart)
        {
            var total = 0;
            foreach (var item in cart.CartItems)
            {
                total += (item.Price * item.Count);
            }

            return total;
        }
    }
}