using GameplaysApi.Models;

namespace GameplaysApi.Interfaces
{
    public interface IUsersRepository
    {
        Task<User?> GetUserByIdAsync(int userId);
        Task DeleteUserAsync(User user);
        Task UpdateUserAsync(User user);
    }
}
