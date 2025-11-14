using Azure;
using PegasusBackend.DTOs.CarDTOs;
using PegasusBackend.DTOs.MapDTOs;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;
using System.Net;
using System.Text.Json;

namespace PegasusBackend.Services.Implementations
{
    public class CarService(IConfiguration configuration, ILogger<CarService> logger, IHttpClientFactory httpClientFactory) : ICarService
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
        private string? apiKey = configuration["BilUppgifterApiKey"];
        public Task<ServiceResponse<bool>> CreateCar(string regNo)
        {
            
        }

        public async Task<ServiceResponse<CarDto>> GetCarData(string regNo)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://data.biluppgifter.se/api/v1/vehicle/regno/{regNo}");

                request.Headers.Add("Authorization", $"Bearer {apiKey}");
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("User-Agent", "PegasusTransport");

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("Google API returned error status: {StatusCode}", response.StatusCode);

                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ServiceResponse<CarDto>.FailResponse(
                        response.StatusCode,
                        $"¨Biluppgifter API error: {errorContent}"
                    );
                }
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
     
                var wrapper = JsonSerializer.Deserialize<BilUppgifterResponseWrapper>(jsonResponse, options);

                var car = new CarDto
                {
                    Color = wrapper!.Vehicle.Color,
                    Make = wrapper.Vehicle.Make,
                    Model = wrapper.Vehicle.Model,
                    RegNr = wrapper.Vehicle.RegNr,
                    Capacity = new TechnicalDto
                    {
                        NumberOfPassengers = wrapper.Vehicle.Capacity.NumberOfPassengers
                    }

                };

                return ServiceResponse<CarDto>.SuccessResponse(
                    HttpStatusCode.OK,
                    car
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while fetching car data");
                return ServiceResponse<CarDto>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "An error occurred while processing your request"
                );
            }
        }
    }
}
