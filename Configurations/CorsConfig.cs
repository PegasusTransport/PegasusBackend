namespace PegasusBackend.Configurations
{
    public static class CorsConfig
    {
        public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy
                        .WithOrigins(
                        "https://localhost:7051",
                        "http://localhost:5173", 
                        "https://pegasustransportportal.azurewebsites.net", 
                        "https://pegasustransport.azurewebsites.net", 
                        "https://pegasusportal.onrender.com",
                        "https://pegasusfrontend.onrender.com", 
                        "https://portal.pegasustransport.se",
                        "https://api.pegasustransport.se",
                        "https://pegasusmvc.onrender.com",
                        "https://pegasustransport.se/"
                        ) 
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials(); 
                });
            });
            return services;
        }
    }
}
