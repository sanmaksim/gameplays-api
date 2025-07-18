using GameplaysApi.Models;

namespace GameplaysApi.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task AddRefreshTokenAsync(RefreshToken refreshToken);
        Task<RefreshToken?> GetRefreshTokenAsync(int userId, string hashedToken);
    }
}
