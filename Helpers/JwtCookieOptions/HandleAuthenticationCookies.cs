using PegasusBackend.Data;

namespace PegasusBackend.Helpers.JwtCookieOptions
{
    public class HandleAuthenticationCookies
    {
        public static void SetAuthenticationCookie(HttpContext httpContext, string accessToken, string refreshToken, AppDBContext context)
        {
            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                throw new ArgumentException("Tokens cannot be null or empty");
            }

            httpContext.Response.Cookies.Append("accessToken", accessToken, CookieOptionsConfig.AccessTokenCookie());
            httpContext.Response.Cookies.Append("refreshToken", refreshToken, CookieOptionsConfig.AccessTokenCookie());
        }
        public static void ClearAuthenticationCookies(HttpContext httpContext)
        {

            httpContext.Response.Cookies.Delete("accessToken", CookieOptionsConfig.AccessTokenCookie());
            httpContext.Response.Cookies.Delete("refreshToken", CookieOptionsConfig.RefreshTokenCookie());
        }
    }
}
