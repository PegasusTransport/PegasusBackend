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
            var priceResponse = await TaxiMeterPrice(durationMinutes ?? 0, distanceKm ?? 0);
            var totalPrice = priceResponse.Data;

            // Check if trip starts OR ends at Arlanda and the other location is in the zone
            bool pickupIsArlanda = pickUpAdress.Contains("arlanda", StringComparison.OrdinalIgnoreCase);
            bool dropoffIsArlanda = dropoffAdress.Contains("arlanda", StringComparison.OrdinalIgnoreCase);
            bool pickupInZone = ZoneHelper.ArlandaZone(pickUpAdress);
            bool dropoffInZone = ZoneHelper.ArlandaZone(dropoffAdress);

            // Apply zone price if:
            // 1. Pickup is Arlanda AND dropoff is in zone, OR
            // 2. Dropoff is Arlanda AND pickup is in zone
            bool shouldApplyZonePrice = (pickupIsArlanda && dropoffInZone) ||
                                       (dropoffIsArlanda && pickupInZone);

            if (shouldApplyZonePrice)
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

            // Collect all addresses in the trip
            var allAddresses = new List<string>
            {
                Dto.PickupAdress
            };

            if (!string.IsNullOrEmpty(Dto.FirstStopAdress))
                allAddresses.Add(Dto.FirstStopAdress);

            if (!string.IsNullOrEmpty(Dto.SecondStopAdress))
                allAddresses.Add(Dto.SecondStopAdress);

            allAddresses.Add(Dto.DropoffAdress);

            // Check if ANY address in the trip is Arlanda
            bool hasArlandaStop = allAddresses.Any(addr =>
                addr.Contains("arlanda", StringComparison.OrdinalIgnoreCase));

            // Check if all OTHER addresses (excluding Arlanda) are in the zone
            bool allOtherAddressesInZone = allAddresses
                .Where(addr => !addr.Contains("arlanda", StringComparison.OrdinalIgnoreCase))
                .All(addr => ZoneHelper.ArlandaZone(addr));

            // If trip has Arlanda AND all other stops are in zone, use zone pricing logic
            bool useZonePricing = hasArlandaStop && allOtherAddressesInZone;

            // No stops - direct trip
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

            // Calculate first segment
            var firstPart = await StopPriceCalculator(
                Dto.PickupAdress.ToLower().Trim(),
                Dto.FirstStopAdress.ToLower().Trim(),
                Dto.FirstStopDurationMinutes,
                Dto.FirstStopDistanceKm,
                zonePrice
            );

            total += firstPart.Data;

            // Calculate remaining segments
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

                total += secondPart.Data + thirdPart.Data;
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

            // If using zone pricing and total exceeds zone price, cap at zone price
            if (useZonePricing && total > zonePrice)
            {
                return ServiceResponse<decimal>.SuccessResponse(
                    HttpStatusCode.OK,
                    zonePrice,
                    "Resan inkluderar Arlanda och är inom zonen. Zonpris tillämpas."
                );
            }

            return ServiceResponse<decimal>.SuccessResponse(
                HttpStatusCode.OK,
                total,
                "Pris beräknat med stopp."
            );
        }
    }
}