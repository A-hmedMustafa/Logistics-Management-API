namespace Logis.Api.Auth
{
    public static class AuthCookie
    {
        public const string RefreshTokenName = "refresh_token";
        public const string RefreshPath = "/api/Auth/refresh";

        public static CookieOptions BuildRefreshCookieOptions(HttpRequest request, DateTime expiresAtUtc)
        {
            return new CookieOptions
            {
                Path = RefreshPath,
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Expires = expiresAtUtc,
                Secure = request.IsHttps
            };
        }

        public static CookieOptions DeleteRefreshCookieOptions(HttpRequest request)
        {
            return new CookieOptions
            {
                Path = RefreshPath,
                HttpOnly = true,
                Secure = request.IsHttps,
                SameSite = SameSiteMode.Strict
            };
        }
    }
}
