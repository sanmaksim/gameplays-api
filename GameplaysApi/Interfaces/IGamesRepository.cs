using GameplaysApi.Models;

namespace GameplaysApi.Interfaces
{
    public interface IGamesRepository
    {
        Task<Game?> GetGameByExternalIdAsync(int externalGameId);
    }
}
