using Microsoft.OpenApi.Models;
using PegasusBackend.Configurations;
using Scalar.AspNetCore;

namespace PegasusBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddConnectionString(builder.Configuration);
            builder.Services.AddApplicationServices(); // Alla DIs ska in hit!
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
