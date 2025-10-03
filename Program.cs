using Microsoft.OpenApi.Models;
using PegasusBackend.Configurations;
using RestrurantPG.Configurations;
using Scalar.AspNetCore;

namespace PegasusBackend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddConnectionString(builder.Configuration);
            builder.Services.AddApplicationServices(); // Alla DIs ska in hit!

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Pegasus Backend API",
                    Version = "v1"
                });
            });
            var app = builder.Build();

            // Create Roles for identity
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    await RoleSeeder.CreateRolesAsync(services);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Ett fel uppstod när roller skulle skapas");
                }
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pegasus Backend API v1");
                });

                app.MapScalarApiReference(options =>
                {
                    options.Title = "Pegasus backend";
                    options.Theme = ScalarTheme.BluePlanet;
                    options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
                    options.OpenApiRoutePattern = "/swagger/v1/swagger.json";
                });
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
