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
        Task<List<UserPlaysDto>> GetPlaysAsync(int userId, int statusId);
        Task<Play?> GetPlayByUserIdAndInternalGameIdAsync(int userId, int internalGameId);
    }
}
