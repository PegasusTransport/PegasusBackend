using PegasusBackend.Helpers.ZoneHelper;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Services.Interfaces;
using PegasusBackend.Responses;
using PegasusBackend.DTOs.TaxiDTOs;
using System.Net;

namespace PegasusBackend.Services.Implementations
{
    public class PriceService : IPriceService
    {
        private readonly IAdminRepo _adminRepo;

        public PriceService(IAdminRepo adminRepo)
        {
            _adminRepo = adminRepo;
        }

        public async Task<ServiceResponse<decimal>> TaxiMeterPrice(
           decimal? durationMinutes,
           decimal? distanceKm)
        {
            var prices = await _adminRepo.GetTaxiPricesAsync();

            if (prices is null)
            {
                return ServiceResponse<decimal>.FailResponse(
                    HttpStatusCode.NotFound,
                    "Didnt found the price in the database!"
                );
            }

            var price = (prices.KmPrice * (distanceKm ?? 0)) +
                (prices.MinutePrice * (durationMinutes ?? 0)) +
                prices.StartPrice;

            return ServiceResponse<decimal>.SuccessResponse(
                HttpStatusCode.OK,
                price,
                "Priset hämtas"
            );
        }

        public async Task<ServiceResponse<decimal>> StopPriceCalculator(
        string pickUpAdress,
        string dropoffAdress,
        decimal? durationMinutes,
        decimal? distanceKm,
        decimal zonePrice)
        {
            var prices = await _adminRepo.GetTaxiPricesAsync();

            if (prices is null)
            {
                return ServiceResponse<decimal>.FailResponse(
                    HttpStatusCode.NotFound,
                    "Couldnt get data from database"
                );
            }

            var meterResponse = await TaxiMeterPrice(durationMinutes ?? 0, distanceKm ?? 0);
            var totalPrice = meterResponse.Data;

            bool pickupIsArlanda = pickUpAdress.Contains("arlanda", StringComparison.OrdinalIgnoreCase);
            bool dropoffIsArlanda = dropoffAdress.Contains("arlanda", StringComparison.OrdinalIgnoreCase);

            bool pickupInZone = ZoneHelper.ArlandaZone(pickUpAdress);
            bool dropoffInZone = ZoneHelper.ArlandaZone(dropoffAdress);

            bool eligibleForZonePrice =
                (pickupIsArlanda && dropoffInZone) ||
                (dropoffIsArlanda && pickupInZone);

            if (eligibleForZonePrice)
            {
                if (totalPrice > zonePrice)
                {
                    return ServiceResponse<decimal>.SuccessResponse(
                        HttpStatusCode.OK,
                        zonePrice,
                        "Fixed price applies for trips to or from Arlanda within the zone.\r\n"
                    );
                }
            }

            return ServiceResponse<decimal>.SuccessResponse(
                HttpStatusCode.OK,
                totalPrice,
                "Taxameter price applies!"
            );
        }

        public async Task<ServiceResponse<decimal>> CalculateTotalPriceAsync(PriceCalculationRequestDto Dto)
        {
            var prices = await _adminRepo.GetTaxiPricesAsync();
            if (prices is null)
            {
                return ServiceResponse<decimal>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "Couldnt get the prcie from database"
                );
            }

            var zonePrice = prices.ZonePrice;
            decimal total = 0;

            if (string.IsNullOrWhiteSpace(Dto.FirstStopAdress))
            {
                var singleTrip = await StopPriceCalculator(
                    Dto.PickupAdress,
                    Dto.DropoffAdress,
                    Dto.LastDurationMinutes,
                    Dto.LastDistanceKm,
                    zonePrice
                );

                return singleTrip;
            }

            var firstPart = await StopPriceCalculator(
                Dto.PickupAdress,
                Dto.FirstStopAdress,
                Dto.FirstStopDurationMinutes,
                Dto.FirstStopDistanceKm,
                zonePrice
            );

            total += firstPart.Data;

            if (!string.IsNullOrWhiteSpace(Dto.SecondStopAdress))
            {
                var secondPart = await StopPriceCalculator(
                    Dto.FirstStopAdress,
                    Dto.SecondStopAdress,
                    Dto.SecondStopDurationMinutes,
                    Dto.SecondStopDistanceKm,
                    zonePrice
                );

                total += secondPart.Data;

                var thirdPart = await StopPriceCalculator(
                    Dto.FirstStopAdress,
                    Dto.SecondStopAdress,
                    Dto.LastDurationMinutes,
                    Dto.LastDistanceKm,
                    zonePrice
                );

                total += thirdPart.Data;
            }
            else
            {
                var lastPart = await StopPriceCalculator(
                    Dto.FirstStopAdress,
                    Dto.DropoffAdress,
                    Dto.LastDurationMinutes,
                    Dto.LastDistanceKm,
                    zonePrice
                );

                total += lastPart.Data;
            }

            return ServiceResponse<decimal>.SuccessResponse(
                HttpStatusCode.OK,
                total,
                "Price calculated with stops (taximeter + zone price per segment)."
            );
        }
    }
}