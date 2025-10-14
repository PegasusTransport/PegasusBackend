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
                    "Kunde inte hämta priset från databasen."
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
                    Console.WriteLine($"[DEBUG] ZONPRIS tillämpas ({zonePrice}kr) " +
                                      $"för resa mellan '{pickUpAdress}' och '{dropoffAdress}'");
                    return ServiceResponse<decimal>.SuccessResponse(
                        HttpStatusCode.OK,
                        zonePrice,
                        "Fastpris tillämpas för resa till eller från Arlanda inom zon."
                    );
                }
            }

            Console.WriteLine($"[DEBUG] TAXAMETERPRIS tillämpas ({totalPrice}kr) " +
                              $"för resa mellan '{pickUpAdress}' och '{dropoffAdress}'");
            return ServiceResponse<decimal>.SuccessResponse(
                HttpStatusCode.OK,
                totalPrice,
                "Taxameterpris tillämpas."
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
            decimal total = 0;

            Console.WriteLine($"[DEBUG] Start prisberäkning. Pickup: '{Dto.PickupAdress}', Dropoff: '{Dto.DropoffAdress}'");

            if (string.IsNullOrWhiteSpace(Dto.FirstStopAdress))
            {
                var singleTrip = await StopPriceCalculator(
                    Dto.PickupAdress,
                    Dto.DropoffAdress,
                    Dto.LastDurationMinutes,
                    Dto.LastDistanceKm,
                    zonePrice
                );

                Console.WriteLine($"[DEBUG] Enkel resa total: {singleTrip.Data}kr");
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
            Console.WriteLine($"[DEBUG] Första del: {firstPart.Data}kr ({Dto.PickupAdress} → {Dto.FirstStopAdress})");

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
                Console.WriteLine($"[DEBUG] Andra del: {secondPart.Data}kr ({Dto.FirstStopAdress} → {Dto.SecondStopAdress})");

                var thirdPart = await StopPriceCalculator(
                    Dto.SecondStopAdress,
                    Dto.DropoffAdress,
                    Dto.LastDurationMinutes,
                    Dto.LastDistanceKm,
                    zonePrice
                );

                total += thirdPart.Data;
                Console.WriteLine($"[DEBUG] Tredje del: {thirdPart.Data}kr ({Dto.SecondStopAdress} → {Dto.DropoffAdress})");
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
                Console.WriteLine($"[DEBUG] Sista del: {lastPart.Data}kr ({Dto.FirstStopAdress} → {Dto.DropoffAdress})");
            }

            Console.WriteLine($"[DEBUG] TOTALT pris: {total}kr");
            return ServiceResponse<decimal>.SuccessResponse(
                HttpStatusCode.OK,
                total,
                "Pris beräknat med stopp (taxameter + zonpris per segment)."
            );
        }
    }
}