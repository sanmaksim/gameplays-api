using GameplaysApi.Models;

namespace GameplaysApi.Interfaces
{
    public interface IUsersRepository
    {
        Task AddUserAsync(User user);
        Task DeleteUserAsync(User user);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByNameAsync(string normalizedUsername);
        Task UpdateUserAsync(User user);
    }
}
