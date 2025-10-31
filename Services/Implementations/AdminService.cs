using Microsoft.Extensions.Options;
using PegasusBackend.Configurations;
using PegasusBackend.DTOs.TaxiDTOs;
using PegasusBackend.Models;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;
using System.Net;

namespace PegasusBackend.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepo _adminRepo;
        private readonly ILogger<AdminService> _logger;
        private readonly PaginationSettings _paginationSettings;

        public AdminService(IAdminRepo adminRepo, ILogger<AdminService> logger, IOptions<PaginationSettings> paginationOptions)
        {
            _adminRepo = adminRepo;
            _logger = logger;
            _paginationSettings = paginationOptions.Value;
        }

        // En admin ska kunna se boking by id!
        // ha en put på booking by id!
        // ha en delete på booking by id!
        public async Task<ServiceResponse<TaxiSettings>> GetTaxiPricesAsync()
        {
            var taxiSettings = await _adminRepo.GetTaxiPricesAsync();

            if (taxiSettings is null)
                return ServiceResponse<TaxiSettings>.FailResponse(
                    HttpStatusCode.NotFound,
                    "Couldnt find any price list in database!"
                );

            return ServiceResponse<TaxiSettings>.SuccessResponse(
                HttpStatusCode.OK, 
                taxiSettings,
                "Retriving the lates price settings!"
            );
        }

        public async Task<ServiceResponse<TaxiSettings>> CreatePricesAsync(NewTaxiSettingsDTO taxiSettingsDTO)
        {
            try
            {
                var newSettings = new TaxiSettings
                {
                    ZonePrice = taxiSettingsDTO.ZonePrice,
                    KmPrice = taxiSettingsDTO.KmPrice,
                    StartPrice = taxiSettingsDTO.StartPrice,
                    MinutePrice = taxiSettingsDTO.MinutePrice,
                    UpdatedAt = DateTime.UtcNow
                };

                await _adminRepo.CreateTaxiPricesAsync(newSettings);

                return ServiceResponse<TaxiSettings>.SuccessResponse(
                    HttpStatusCode.OK,
                    newSettings,
                    "PriceSettings is now updated!"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new taxi prices");
                return ServiceResponse<TaxiSettings>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "Something went wrong");
            }
        }
    }
}
