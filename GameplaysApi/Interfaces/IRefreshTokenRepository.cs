using GameplaysApi.Models;

namespace GameplaysApi.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task AddRefreshTokenAsync(RefreshToken refreshToken);
        Task<RefreshToken?> GetRefreshTokenAsync(string hashedToken);
        Task UpdateRefreshTokenAsync(RefreshToken refreshToken, string hashedString, DateTime expiresAt);
        Task DeleteRefreshTokenAsync(RefreshToken refreshToken);
    }
}
