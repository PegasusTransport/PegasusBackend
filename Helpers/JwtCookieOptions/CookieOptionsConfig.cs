using Microsoft.Extensions.Configuration;

namespace PegasusBackend.Helpers.JwtCookieOptions
{
    public class CookieOptionsConfig
    {
        public static CookieOptions AccessTokenCookie(IConfiguration configuration)
        {
            var expire = configuration.GetValue<int>("JwtSetting:AccessTokenExpire");
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddMinutes(expire),
            };
        }
        public static CookieOptions RefreshTokenCookie(IConfiguration configuration)
        {
            var expire = configuration.GetValue<int>("JwtSetting:RefreshTokenExpire");

            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(expire)
            };
        }
    }
}
