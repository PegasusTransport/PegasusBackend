using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PegasusBackend.Services.Interfaces;
using System.Net;
using System.Text.Json;

namespace PegasusBackend.Attributes
{
    /// Attribute to mark an action as idempotent.
    /// Prevents duplicate operations by checking idempotency key in request header.
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class IdempotentAttribute : Attribute, IAsyncActionFilter
    {
        private const string IdempotencyKeyHeader = "Idempotency-Key";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var idempotencyService = context.HttpContext.RequestServices
                .GetRequiredService<IIdempotencyService>();

            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<IdempotentAttribute>>();

            // Extract idempotency key from header
            if (!context.HttpContext.Request.Headers.TryGetValue(IdempotencyKeyHeader, out var idempotencyKey) ||
                string.IsNullOrWhiteSpace(idempotencyKey))
            {
                logger.LogWarning("Request to idempotent endpoint without Idempotency-Key header");

                context.Result = new BadRequestObjectResult(new
                {
                    message = $"{IdempotencyKeyHeader} header is required for this operation to prevent duplicates."
                });
                return;
            }

            var key = idempotencyKey.ToString();

            // Check if this key has been used before
            var existingRecord = await idempotencyService.GetExistingRecordAsync(key);

            if (existingRecord != null)
            {
                // Return cached response
                logger.LogInformation(
                    "Idempotent request detected for key: {Key}. Returning cached response for booking: {BookingId}",
                    key,
                    existingRecord.BookingId);

                var cachedData = JsonSerializer.Deserialize<object>(
                    existingRecord.ResponseData,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                context.Result = new ObjectResult(cachedData)
                {
                    StatusCode = existingRecord.StatusCode
                };
                return;
            }

            // Store key in HttpContext for later use
            context.HttpContext.Items["IdempotencyKey"] = key;

            // Continue with the action
            var executedContext = await next();

            // After action executes successfully, save the idempotency record
            if (executedContext.Result is ObjectResult objectResult &&
                objectResult.StatusCode >= 200 &&
                objectResult.StatusCode < 300)
            {
                try
                {
                    // Extract booking ID from response
                    var responseValue = objectResult.Value;
                    int? bookingId = ExtractBookingId(responseValue);

                    if (bookingId.HasValue)
                    {
                        await idempotencyService.CreateRecordAsync(
                            key: key,
                            bookingId: bookingId.Value,
                            responseData: responseValue!,
                            statusCode: objectResult.StatusCode ?? 200,
                            expirationHours: 24
                        );

                        logger.LogInformation(
                            "Saved idempotency record for booking: {BookingId}, key: {Key}",
                            bookingId.Value,
                            key);
                    }
                }
                catch (Exception ex)
                {
                    // Log but don't fail the request - operation completed successfully
                    logger.LogError(ex,
                        "Failed to save idempotency record for key: {Key}. " +
                        "Operation completed successfully but may be vulnerable to duplicates on retry.",
                        key);
                }
            }
        }

        /// <summary>
        /// Extract booking ID from response object
        /// Handles different response structures
        /// </summary>
        private int? ExtractBookingId(object? response)
        {
            if (response == null)
            {
                Console.WriteLine("⚠️ Response is null");
                return null;
            }

            try
            {
                var json = JsonSerializer.Serialize(response);
                Console.WriteLine($"Response JSON: {json.Substring(0, Math.Min(200, json.Length))}...");

                // Deserialize to dynamic object with case-insensitive options
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                using var doc = JsonDocument.Parse(json);

                // Search through all properties recursively
                return FindBookingIdRecursive(doc.RootElement);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 Error extracting BookingId: {ex.Message}");
                return null;
            }
        }

        private int? FindBookingIdRecursive(JsonElement element)
        {
            // If this element is an object, check its properties
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in element.EnumerateObject())
                {
                    // Check if property name matches (case-insensitive)
                    if (property.Name.Equals("bookingId", StringComparison.OrdinalIgnoreCase))
                    {
                        if (property.Value.ValueKind == JsonValueKind.Number)
                        {
                            var id = property.Value.GetInt32();
                            Console.WriteLine($"✅ Found {property.Name}: {id}");
                            return id;
                        }
                    }

                    // Recursively search nested objects
                    var nested = FindBookingIdRecursive(property.Value);
                    if (nested.HasValue)
                        return nested;
                }
            }

            return null;
        }
    }
}