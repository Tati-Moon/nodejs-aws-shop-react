using CartService.Domain.Entity;
using CartService.Domain.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CartService.Domain.Services
{

    public class ProductService(DatabaseContext context) : IProductService
    {
        private readonly DatabaseContext _context = context;

        public async Task<Product> GetProductByIdAsync(Guid id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            return await _context.Products.ToListAsync();
        }
    }
}
