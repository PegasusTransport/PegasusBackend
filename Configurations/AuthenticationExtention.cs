using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace PegasusBackend.Configurations
{
    public static class AuthenticationExtention
    {
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = configuration["JwtSetting:Issuer"],  // ← Settings, inte Setting

                        ValidateAudience = true,
                        ValidAudience = configuration["JwtSetting:Audience"],

                        ValidateLifetime = true,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(configuration["JwtSetting:Key"]!)  
                        ),

                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            // Check first in header after Bearer: Token
                            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
                            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                            {
                                context.Token = authHeader["Bearer ".Length..].Trim();
                            }
                            // If not, find access token in Cookies
                            else if (context.Request.Cookies.ContainsKey("accessToken"))
                            {
                                context.Token = context.Request.Cookies["accessToken"];
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization();
            return services; 
        }
    }
}