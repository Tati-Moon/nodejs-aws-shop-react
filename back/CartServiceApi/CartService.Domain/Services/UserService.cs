using CartService.Domain.Entity;
using CartService.Domain.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CartService.Domain.Services
{
    public class UserService(DatabaseContext context) : IUserService
    {
        private readonly DatabaseContext _context = context;

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> GetUserByIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User> GetUserByNameAsync(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentNullException(nameof(userName));
            }

            return await _context.Users.FirstOrDefaultAsync(s => s.Login.ToLower() == userName.ToLower());
        }

        public async Task<bool> ValidateUserCredentialsAsync(string userName, string password)
        {
            var user = await GetUserByNameAsync(userName);
            return user != null && user.Password == password;
        }
    }
}
