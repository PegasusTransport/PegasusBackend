using Azure;
using Org.BouncyCastle.Crypto;
using PegasusBackend.DTOs.CarDTOs;
using PegasusBackend.DTOs.DriverDTO;
using PegasusBackend.DTOs.MapDTOs;
using PegasusBackend.Models;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;
using System.Net;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PegasusBackend.Services.Implementations
{
    public class CarService(IConfiguration configuration, ILogger<CarService> logger, IHttpClientFactory httpClientFactory, ICarRepo carRepo) : ICarService
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
        private string? apiKey = configuration["BilUppgifterApiKey"];

        public async Task<Cars?> CreateOrFindCarWithDriver(string regNo, Guid driverId, CreateRequestDriverDto? driverRequest = null)
        {
            var existingCar = await carRepo.FindCarByRegNumberAsync(regNo);
            if (existingCar != null)
            {
                existingCar.DriverIdFk = driverId;
                await carRepo.UpdateCar(existingCar);
                logger.LogInformation("Car with registration {RegNo} exists, updated driver to {DriverId}", regNo, driverId);
                return existingCar;
            }

            var car = new Cars
            {
                LicensePlate = regNo.ToUpper(),
                Make = driverRequest?.CarMake ?? "Unknown", 
                Model = driverRequest?.CarModel ?? "Unknown",
                Type = driverRequest?.CarType ?? "Sedan",
                Capacity = driverRequest?.CarCapacity ?? 4,
                DriverIdFk = driverId
            };

            try
            {
                await carRepo.SaveCar(car);
                logger.LogInformation("Successfully created new car with registration {RegNo} for driver {DriverId}", regNo, driverId);
                return car;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to save car with registration {RegNo}", regNo);
                return null;
            }
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
