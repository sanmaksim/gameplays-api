using GameplaysApi.DTOs;
using GameplaysApi.Models;

namespace GameplaysApi.Interfaces
{
    public interface IPlaysRepository
    {
        Task AddPlayAsync(Play play);
        Task RemovePlayAsync(Play play);
        Task UpdatePlayStatusAsync(Play play, int newStatus);
        Task<Play?> GetPlayByIdAsync(int playId);
        Task<PlayStatusDto?> GetPlayByUserIdAndExternalGameIdAsync(int userId, int externalGameId);
        Task<List<UserPlaysDto>> GetPlaysByUserIdAsync(int userId);
        Task<Play?> GetPlayByUserIdAndInternalGameIdAsync(int userId, int internalGameId);
    }
}
