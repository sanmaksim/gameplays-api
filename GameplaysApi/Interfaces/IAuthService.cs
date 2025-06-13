using GameplaysApi.Models;

namespace GameplaysApi.Interfaces
{
    public interface IAuthService
    {
        void CreateAuthCookie(User user, HttpResponse response);
        void DeleteAuthCookie(HttpResponse response);
        string GetCurrentUserId();
    }
}
