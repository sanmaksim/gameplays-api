namespace GameplaysApi.Services
{
    public class CookieService
    {
        public void CreateCookie(HttpResponse response, string cookieName, string cookieValue, int expDays)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(expDays),
                HttpOnly = true,
                Path = "/",
                Secure = true,
                SameSite = SameSiteMode.None
            };

            response.Cookies.Append(cookieName, cookieValue, cookieOptions);
        }

        public void DeleteCookie(HttpResponse response, string cookieName)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Path = "/",
                Secure = true,
                SameSite = SameSiteMode.None
            };

            response.Cookies.Delete(cookieName, cookieOptions);
        }
    }
}
