namespace GameplaysApi.Interfaces
{
    public interface ICookieService
    {
        void CreateCookie(HttpResponse response, string cookieName, string cookieValue, int expDays);
        void DeleteCookie(HttpResponse response, string cookieName);
    }
}
