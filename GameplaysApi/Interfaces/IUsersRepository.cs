using GameplaysApi.Models;

namespace GameplaysApi.Interfaces
{
    public interface IUsersRepository
    {
        Task<User?> GetUserByIdAsync(int userId);
        Task UpdateUserAsync(User user);
    }
}
