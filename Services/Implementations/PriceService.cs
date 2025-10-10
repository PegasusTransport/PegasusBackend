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

            return ServiceResponse<decimal>.SuccessResponse(HttpStatusCode.OK,
                price,
                "Priset hämtas");
        }

        public async Task<ServiceResponse<decimal>> StopPriceCalculator(
        string pickUpAdress,
        string dropoffAdress,
        decimal? durationMinutes,
        decimal? distanceKm,
        decimal zonePrice)
        {
            var priceResponse = await TaxiMeterPrice(durationMinutes ?? 0, distanceKm ?? 0);
            var totalPrice = priceResponse.Data;

            // När vi implementerar GoogleMaps API, får vi göra en riktig Arlanda-check genom address_components.
            if ((ZoneHelper.ArlandaZone(pickUpAdress) && dropoffAdress.Contains("arlanda", StringComparison.OrdinalIgnoreCase)) ||
                (pickUpAdress.Contains("arlanda", StringComparison.OrdinalIgnoreCase) && ZoneHelper.ArlandaZone(dropoffAdress)))
            {
                if (totalPrice > zonePrice)
                {
                    return ServiceResponse<decimal>.SuccessResponse(
                        HttpStatusCode.OK,
                        zonePrice,
                        "Priset är över zonpris, zonpris används istället."
                    );
                }
                else
                {
                    return ServiceResponse<decimal>.SuccessResponse(
                        HttpStatusCode.OK,
                        totalPrice,
                        "Upphämtning/avlämning är inom zon. Priset blev under zonpris!"
                    );
                }
            }

            return ServiceResponse<decimal>.SuccessResponse(
                HttpStatusCode.OK,
                totalPrice,
                "Upphämtning/avlämning är inte inom zon. Totalpris gäller!"
            );
        }

        // Ta emot lista av adresser istället för att få 3 adresser. Då kan man ha valfri antal stopp!!
        public async Task<ServiceResponse<decimal>> CalculateTotalPriceAsync(PriceCalculationRequestDto Dto)
        {
            var prices = await _adminRepo.GetTaxiPricesAsync();

            if (prices is null)
            {
                return ServiceResponse<decimal>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "Kunde inte hämta priset från databasen."
                );
            }

            var zonePrice = prices.ZonePrice;

            if (Dto.FirstStopAdress is null)
            {
                var totalPriceResponse = await StopPriceCalculator(
                    Dto.PickupAdress.ToLower().Trim(),
                    Dto.DropoffAdress.ToLower().Trim(),
                    Dto.LastDurationMinutes,
                    Dto.LastDistanceKm,
                    zonePrice
                );

                return ServiceResponse<decimal>.SuccessResponse(
                    HttpStatusCode.OK,
                    totalPriceResponse.Data,
                    "Pris beräknat utan stopp."
                );
            }

            decimal total = 0;

            // Räknar mellan pickup till stopp, och stopp till dropoff.
            var firstPart = await StopPriceCalculator(
                Dto.PickupAdress.ToLower().Trim(),
                Dto.FirstStopAdress.ToLower().Trim(),
                Dto.FirstStopDurationMinutes,
                Dto.FirstStopDistanceKm,
                zonePrice
            );

            total += firstPart.Data;

            // Om andra stopp finns, beräkna tre delar istället för två.
            if (Dto.SecondStopAdress is not null)
            {
                var secondPart = await StopPriceCalculator(
                    Dto.FirstStopAdress.ToLower().Trim(),
                    Dto.SecondStopAdress.ToLower().Trim(),
                    Dto.SecondStopDurationMinutes,
                    Dto.SecondStopDistanceKm,
                    zonePrice
                );

                var thirdPart = await StopPriceCalculator(
                    Dto.SecondStopAdress.ToLower().Trim(),
                    Dto.DropoffAdress.ToLower().Trim(),
                    Dto.LastDurationMinutes,
                    Dto.LastDistanceKm,
                    zonePrice
                );

                total += (secondPart.Data ) + (thirdPart.Data);
            }
            else
            {
                var secondPart = await StopPriceCalculator(
                    Dto.FirstStopAdress.ToLower().Trim(),
                    Dto.DropoffAdress.ToLower().Trim(),
                    Dto.LastDurationMinutes,
                    Dto.LastDistanceKm,
                    zonePrice
                );

                total += secondPart.Data;
            }

            return ServiceResponse<decimal>.SuccessResponse(
                HttpStatusCode.OK,
                total,
                "Pris beräknat med stopp."
            );
        }

    }
}
