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
                        .WithOrigins("http://127.0.0.1:5500") // Testing b
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials(); 
                });
            });
            return services;
        }
    }
}
