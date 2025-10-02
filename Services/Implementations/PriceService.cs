using PegasusBackend.Helpers.ZoneHelper;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Services.Interfaces;

namespace PegasusBackend.Services.Implementations
{
    public class PriceService : IPriceService
    {
        private readonly IAdminRepo _adminRepo;

        public PriceService(IAdminRepo adminRepo)
        {
            _adminRepo = adminRepo;
        }

        public async Task<(bool Success, decimal? Price, string Message)> TaxiMeterPrice(
           decimal? durationMinutes,
           decimal? distanceKm)
        {
            var prices = await _adminRepo.GetTaxiPricesAsync();

            if (prices is null)
            {
                return (false, null, "kunde inte hämta priset från databasen.");
            }

            var price = (prices.KmPrice * distanceKm) + (prices.MinutePrice * durationMinutes) + prices.StartPrice;

            return (true, price, "Priset har räknats ut!");
        }

        public async Task<(bool Success, decimal? Price, string Message)> StopPriceCalculator(string pickUpAdress, string dropoffAdress, decimal? durationMinutes, decimal? distanceKm, decimal zonePrice)
        {
            var price = await TaxiMeterPrice(durationMinutes ?? 0, distanceKm ?? 0);
            var totalprice = price.Price ?? 0;

            // När vi implementerar GoogleMaps Api, får vi göra en riktig arlanda check genom adress_components.
            if ((ZoneHelper.ArlandaZone(pickUpAdress) && dropoffAdress.Contains("arlanda")) ||
                (pickUpAdress.Contains("arlanda") && ZoneHelper.ArlandaZone(dropoffAdress)))
            {
                if (totalprice > zonePrice)
                {
                    return (true, zonePrice, "Upphämtning/avlämning är inom zon. Zonpriset gäller!");
                }
                else
                {
                    return (true, totalprice, "Upphämtning/avlämning är inom zon. Priset blev under zonpris! ");
                }
            }

            return (true, totalprice, "Upphämtning/avlämning är inte inom zon. Total pris gäller!");
        }



        public async Task<(bool Success, decimal? Price, string Massange)> CalculateTotalPriceAsync(
            string pickupAdress,
            string dropoffAdress,
            decimal distanceKm,
            decimal durationMinutes,
            string? firstStopAdress,
            decimal? firstStopDistanceKm,
            decimal? firstStopDurationMinutes,
            string? secondStopAdress,
            decimal? secondStopDistanceKm,
            decimal? secondStopDurationMinutes)
        {
            var prices = await _adminRepo.GetTaxiPricesAsync();

            if (prices is null)
            {
                return (false, null, "kunde inte hämta priset från databasen.");
            }

            var zonePrice = prices.ZonePrice;

            if (firstStopAdress is null)
            {
                var totalPrice = await StopPriceCalculator(pickupAdress.ToLower().Trim(), dropoffAdress.ToLower().Trim(), durationMinutes, distanceKm, zonePrice);
                return (true, totalPrice.Price, "Pris beräknat utan stopp.");
            }

            decimal total = 0;

            // Måste räkna mellan första stopp till sista stopp. 
            

            // om andra stopp inte är null....

            // räkna andra segment från första stopp till andra stopp och andra stopp till dropoff. 



            return (true, total, "Pris beräknat med stopp.");
        }

    }
}
