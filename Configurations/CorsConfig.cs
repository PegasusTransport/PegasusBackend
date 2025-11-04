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
                        .WithOrigins("https://localhost:7051", "http://localhost:5173") 
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials(); 
                });
            });
            return services;
        }
    }
}
