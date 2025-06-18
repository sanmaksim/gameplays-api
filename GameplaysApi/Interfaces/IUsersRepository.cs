using GameplaysApi.Models;

namespace GameplaysApi.Interfaces
{
    public interface IUsersRepository
    {
        Task<User?> GetUserByIdAsync(int userId);
    }
}
