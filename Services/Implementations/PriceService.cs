using PegasusBackend.DTOs;
using PegasusBackend.Helpers.ZoneHelper;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Services.Interfaces;
using PegasusBackend.Responses;

namespace PegasusBackend.Services.Implementations
{
    public class PriceService : IPriceService
    {
        private readonly IAdminRepo _adminRepo;

        public PriceService(IAdminRepo adminRepo)
        {
            _adminRepo = adminRepo;
        }

        public async Task<PriceResponse> TaxiMeterPrice(
           decimal? durationMinutes,
           decimal? distanceKm)
        {
            var prices = await _adminRepo.GetTaxiPricesAsync();

            if (prices is null)
            {
                return new PriceResponse
                {
                    Success = false,
                    Price = null,
                    Message = "kunde inte hämta priset från databasen."
                };
            }

            var price = (prices.KmPrice * distanceKm) + (prices.MinutePrice * durationMinutes) + prices.StartPrice;

            return new PriceResponse
            {
                Success = true,
                Price = price,
                Message = "Priset har räknats ut!"
            };
        }

        public async Task<PriceResponse> StopPriceCalculator(string pickUpAdress, string dropoffAdress, decimal? durationMinutes, decimal? distanceKm, decimal zonePrice)
        {
            var price = await TaxiMeterPrice(durationMinutes ?? 0, distanceKm ?? 0);
            var totalprice = price.Price ?? 0;

            // När vi implementerar GoogleMaps Api, får vi göra en riktig arlanda check genom adress_components.
            if ((ZoneHelper.ArlandaZone(pickUpAdress) && dropoffAdress.Contains("arlanda")) ||
                (pickUpAdress.Contains("arlanda") && ZoneHelper.ArlandaZone(dropoffAdress)))
            {
                if (totalprice > zonePrice)
                {
                    return new PriceResponse
                    {
                        Success = true,
                        Price = zonePrice,
                        Message = "Upphämtning/avlämning är inom zon. Zonpriset gäller!"
                    };
                }
                else
                {
                    return new PriceResponse
                    {
                        Success = true,
                        Price = totalprice,
                        Message = "Upphämtning/avlämning är inom zon. Priset blev under zonpris! "
                    };
                }
            }

            return new PriceResponse
            {
                Success = true,
                Price = totalprice,
                Message = "Upphämtning/avlämning är inte inom zon. Total pris gäller!"
            };
        }


        // Ta emot lista av adresser istället för att få 3 adresser. Då kan man ha valfri antal stopp!!
        public async Task<PriceResponse> CalculateTotalPriceAsync(PriceCalculationRequestDto Dto)
        {
            var prices = await _adminRepo.GetTaxiPricesAsync();

            if (prices is null)
            {
                return new PriceResponse
                {
                    Success = false,
                    Price = null,
                    Message = "kunde inte hämta priset från databasen."
                };
            }

            var zonePrice = prices.ZonePrice;

            if (Dto.FirstStopAdress is null)
            {
                var totalPrice = await StopPriceCalculator(Dto.PickupAdress.ToLower().Trim(), Dto.DropoffAdress.ToLower().Trim(), Dto.LastDurationMinutes, Dto.LastDistanceKm, zonePrice);
                return new PriceResponse
                {
                    Success = true,
                    Price = totalPrice.Price,
                    Message = "Pris beräknat utan stopp."
                };
            }

            decimal total = 0;

            // Räknar mellan pickup till stopp, och stopp till dropoff.  
            var FirstPart = await StopPriceCalculator(Dto.PickupAdress.ToLower().Trim(), Dto.FirstStopAdress.ToLower().Trim(), Dto.FirstStopDurationMinutes, Dto.FirstStopDistanceKm, zonePrice);

            total += FirstPart.Price ?? 0;

            // om andra stopp inte är null....
            if (Dto.SecondStopAdress is not null)
            {
                var secondPart = await StopPriceCalculator(Dto.FirstStopAdress.ToLower().Trim(), Dto.SecondStopAdress.ToLower().Trim(), Dto.SecondStopDurationMinutes, Dto.SecondStopDistanceKm, zonePrice);
                var thirdPart = await StopPriceCalculator(Dto.SecondStopAdress.ToLower().Trim(), Dto.DropoffAdress.ToLower().Trim(), Dto.LastDurationMinutes, Dto.LastDistanceKm, zonePrice);
                total += thirdPart.Price ?? 0 + secondPart.Price ?? 0;
            }
            else
            {
                var secondPart = await StopPriceCalculator(Dto.FirstStopAdress.ToLower().Trim(), Dto.DropoffAdress.ToLower().Trim(), Dto.LastDurationMinutes, Dto.LastDistanceKm, zonePrice);
                total += secondPart.Price ?? 0;
            }

            return new PriceResponse
            {
                Success = true,
                Price = total,
                Message = "Pris beräknat med stopp."
            };
        }
    }
}
