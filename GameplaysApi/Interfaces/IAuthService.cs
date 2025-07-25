using GameplaysApi.Models;

namespace GameplaysApi.Interfaces
{
    public interface IAuthService
    {
        void CreateAuthCookie(User user, HttpResponse response);
        Task CreateRefreshTokenCookie(User user, HttpRequest request, HttpResponse response);
        Task UpdateRefreshTokenCookie(User user, HttpRequest request, HttpResponse response, RefreshToken refreshToken);
        void DeleteAuthCookie(HttpResponse response);
        void DeleteRefreshTokenCookie(HttpResponse response);
        string GetCurrentUserId();
    }
}
