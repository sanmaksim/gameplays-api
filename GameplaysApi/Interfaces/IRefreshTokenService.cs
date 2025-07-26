using GameplaysApi.Models;

namespace GameplaysApi.Interfaces
{
    public interface IRefreshTokenService
    {
        Task<string> CreateRefreshToken(User user, HttpRequest request, TimeSpan expiresIn);
        Task<RefreshToken?> GetRefreshToken(string tokenString);
        Task<string> UpdateRefreshToken(User user, HttpRequest request, TimeSpan expiresIn, RefreshToken refreshToken);
        Task DeleteRefreshToken(RefreshToken refreshToken);
    }
}
