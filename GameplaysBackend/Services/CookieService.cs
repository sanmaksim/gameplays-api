namespace GameplaysBackend.Services
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
                Secure = false, // should be true for HTTPS in production
                SameSite = SameSiteMode.Strict
            };

            response.Cookies.Append(cookieName, cookieValue, cookieOptions);
        }

        public void DeleteCookie(HttpResponse response, string cookieName)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Path = "/",
                Secure = false, // should be true for HTTPS in production
                SameSite = SameSiteMode.Strict
            };

            response.Cookies.Delete(cookieName, cookieOptions);
        }
    }
}
