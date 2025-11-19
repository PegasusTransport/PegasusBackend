namespace PegasusBackend.Helpers.JwtCookieOptions
{
    public class CookieOptionsConfig
    {
        public static CookieOptions AccessTokenCookie()
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = CookieExpirationHelper.GetAccessTokenExpiration()
            };
        }

        public static CookieOptions RefreshTokenCookie()
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = CookieExpirationHelper.GetRefreshTokenExpiration()
            };
        }
    }
}