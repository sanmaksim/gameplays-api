namespace GameplaysApi.Interfaces
{
    public interface ICookieService
    {
        void CreateCookie(HttpResponse response, string cookieName, string cookieValue, TimeSpan expiresIn);
        void DeleteCookie(HttpResponse response, string cookieName);
    }
}
