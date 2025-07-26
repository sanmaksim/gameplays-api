using GameplaysApi.Models;

namespace GameplaysApi.Interfaces
{
    public interface IAuthService
    {
        string GetCurrentUserId();
        void CreateAuthCookie(User user, HttpResponse response);
        void DeleteAuthCookie(HttpResponse response);
        Task CreateRefreshTokenCookie(User user, HttpRequest request, HttpResponse response);
        Task UpdateRefreshTokenCookie(User user, HttpRequest request, HttpResponse response, RefreshToken refreshToken);
        void DeleteRefreshTokenCookie(HttpResponse response, RefreshToken refreshToken);
    }
}
