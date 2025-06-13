using GameplaysApi.DTOs;
using GameplaysApi.Models;

namespace GameplaysApi.Interfaces
{
    public interface IPlaysRepository
    {
        Task<User?> GetUserByIdAsync(int id);
        Task<List<UserPlaysDto>> GetPlaysByUserIdAsync(int id);
    }
}
