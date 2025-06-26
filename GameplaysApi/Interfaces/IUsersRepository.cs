using GameplaysApi.Models;

namespace GameplaysApi.Interfaces
{
    public interface IUsersRepository
    {
        Task DeleteUserAsync(User user);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByNameAsync(string username);
        Task UpdateUserAsync(User user);
    }
}
