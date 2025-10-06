using Microsoft.OpenApi.Models;
using PegasusBackend.Configurations;
using Scalar.AspNetCore;

namespace PegasusBackend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddConnectionString(builder.Configuration);
            builder.Services.AddApplicationServices(builder.Configuration); // Alla DIs ska in hit!
            builder.Services.AddJwtAuthentication(builder.Configuration);
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
            // Seed roles
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
                    logger.LogError(ex, "Error while roles seeds");
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
