using CartService.Domain.Entity;

namespace CartService.Domain.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetUsersAsync();
        Task<User> GetUserByIdAsync(Guid id);
        Task<User> GetUserByNameAsync(string userName);
        Task<bool> ValidateUserCredentialsAsync(string userName, string password);
    }
}
