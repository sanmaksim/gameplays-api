namespace GameplaysBackend.Services
{
    public class CookieService
    {
        public void CreateCookie(HttpResponse response, string cookieName, string cookieValue, int expirationDays)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(expirationDays),
                HttpOnly = true,
                Secure = false, // should be true for HTTPS in production
                SameSite = SameSiteMode.Lax // or Strict ???
            };

            response.Cookies.Append(cookieName, cookieValue, cookieOptions);
        }
    }
}
