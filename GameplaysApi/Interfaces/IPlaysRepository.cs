using GameplaysApi.DTOs;
using GameplaysApi.Models;

namespace GameplaysApi.Interfaces
{
    public interface IPlaysRepository
    {
        Task AddPlayAsync(Play play);
        Task RemovePlayAsync(Play play);
        Task UpdatePlayStatusAsync(Play play, int statusId);
        Task<Play?> GetPlayAsync(int playId);
        Task<Play?> GetPlayByUserAndApiGameIdAsync(int userId, int apiGameId);
        Task<List<PlayResponseDto>> GetPlaysByUserAndOptionalIdAsync(int userId, int? apiGameId, int? statusId);
    }
}
