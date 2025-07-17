using GameplaysApi.Models;

namespace GameplaysApi.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task AddRefreshTokenAsync(RefreshToken refreshToken);
    }
}
