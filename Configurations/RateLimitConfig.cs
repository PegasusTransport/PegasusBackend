using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using System.Runtime.CompilerServices;
using System.Threading.RateLimiting;

namespace PegasusBackend.Configurations
{
    public static class RateLimitConfig
    {
        public static IServiceCollection AddRateLimitersPolicys(this IServiceCollection services)
        {

            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                // Get a Ratelimit specific to IP address
                options.AddPolicy("LoginPolicy", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromSeconds(30),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        })); 

            });

            return services;
        }
    }
}
