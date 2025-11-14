using Azure;
using PegasusBackend.DTOs.CarDTOs;
using PegasusBackend.DTOs.MapDTOs;
using PegasusBackend.Models;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;
using System.Net;
using System.Reflection.Metadata;
using System.Text.Json;

namespace PegasusBackend.Services.Implementations
{
    public class CarService(IConfiguration configuration, ILogger<CarService> logger, IHttpClientFactory httpClientFactory, ICarRepo carRepo) : ICarService
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
        private string? apiKey = configuration["BilUppgifterApiKey"];
        public async Task<Cars?> CreateOrFindCar(string regNo)
        {

            var existingCar = await carRepo.FindCarByRegNumberAsync(regNo);
            if (existingCar != null)
            {
                logger.LogWarning("Car exist");
                return existingCar;
            }
            var carDetails = await GetCarData(regNo);

            if (carDetails == null)
            {
                logger.LogError("Biluppgifter API Error");
                return null;
            }
            var car = new Cars
            {
                LicensePlate = regNo,
                Model = carDetails.Model,
                Make = carDetails.Make,
                Type = carDetails.Type,
                Capacity = carDetails.Capacity.NumberOfPassengers,
            };
            await carRepo.SaveCar(car);
            return car;
        }
        public async Task<CarDto?> GetCarData(string regNo)
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
                    return null;
                    
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

                return car;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while fetching car data");
                throw; 
            }   
        }
    }
}
