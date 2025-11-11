using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

public static class RateLimitingConfiguration
{
    public static IServiceCollection AddRateLimitPolicies(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests; // rejection code

            AddAuthPolicy(options);
            AddRegistrationPolicy(options);
            AddBookingPolicy(options);
            AddMapAPIPolicy(options);
            AddPasswordResetPolicy(options);
        });

        return services;
    }
    // Policys
    private static void AddAuthPolicy(RateLimiterOptions options)
    {
        options.AddPolicy("AuthPolicy", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString(),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromSeconds(30),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));
    }

    private static void AddRegistrationPolicy(RateLimiterOptions options)
    {
        options.AddPolicy("RegistrationPolicy", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString(),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 3,
                    Window = TimeSpan.FromHours(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));
    }

    private static void AddBookingPolicy(RateLimiterOptions options)
    {
        options.AddPolicy("BookingPolicy", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString(),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromHours(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));
    }
    private static void AddMapAPIPolicy(RateLimiterOptions options)
    {
        options.AddPolicy("MapApiPolicy", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString(),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromHours(1),
                    QueueLimit = 1,
                    AutoReplenishment = true
                }));
    }
    private static void AddPasswordResetPolicy(RateLimiterOptions options)
    {
        options.AddPolicy("PasswordResetPolicy", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString(),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 3,
                    Window = TimeSpan.FromHours(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));
    }

}
