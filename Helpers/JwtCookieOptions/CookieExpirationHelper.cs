namespace PegasusBackend.Helpers.JwtCookieOptions
{
    public static class CookieExpirationHelper
    {
        private static int _accessTokenExpireMinutes;
        private static int _refreshTokenExpireDays;

        public static void Initialize(IConfiguration configuration)
        {
            _accessTokenExpireMinutes = configuration.GetValue<int>("JwtSetting:AccessTokenExpire");
            _refreshTokenExpireDays = configuration.GetValue<int>("JwtSetting:RefreshTokenExpire");
        }

        public static DateTimeOffset GetAccessTokenExpiration()
        {
            return DateTimeOffset.UtcNow.AddMinutes(_accessTokenExpireMinutes);
        }

        public static DateTimeOffset GetRefreshTokenExpiration()
        {
            return DateTimeOffset.UtcNow.AddDays(_refreshTokenExpireDays);
        }
    }
}