using PegasusBackend.Data;

namespace PegasusBackend.Helpers.JwtCookieOptions
{
    public class HandleAuthenticationCookies
    {
        public static void SetAuthenticationCookie(HttpContext httpContext, string accessToken, string refreshToken)
        {
            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                throw new ArgumentException("Tokens cannot be null or empty");
            }

            httpContext.Response.Cookies.Append(CookieNames.AccessToken, accessToken, CookieOptionsConfig.AccessTokenCookie());
            httpContext.Response.Cookies.Append(CookieNames.RefreshToken, refreshToken, CookieOptionsConfig.RefreshTokenCookie());
        }
        public static void ClearAuthenticationCookies(HttpContext httpContext)
        {

            httpContext.Response.Cookies.Delete(CookieNames.AccessToken, CookieOptionsConfig.AccessTokenCookie());
            httpContext.Response.Cookies.Delete(CookieNames.RefreshToken, CookieOptionsConfig.RefreshTokenCookie());
        }
    }
    
}
