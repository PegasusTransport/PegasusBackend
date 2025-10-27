using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using PegasusBackend.Attributes;

namespace PegasusBackend.Filters
{
    public class IdempotencyHeaderFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Kolla om endpoint har [Idempotent] attribute
            var hasIdempotent = context.MethodInfo
                .GetCustomAttributes(true)
                .Any(x => x is IdempotentAttribute);

            if (hasIdempotent)
            {
                operation.Parameters ??= new List<OpenApiParameter>();

                // Lägg till Idempotency-Key header parameter
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "Idempotency-Key",
                    In = ParameterLocation.Header,
                    Required = true,
                    Description = "Unique key to prevent duplicate requests",
                    Schema = new OpenApiSchema
                    {
                        Type = "string"
                    }
                });
            }
        }
    }
}