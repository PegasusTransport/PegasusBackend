using PegasusBackend.DTOs;
using PegasusBackend.Responses;

public interface IPriceService
{
    Task<PriceResponse> TaxiMeterPrice(
    decimal? durationMinutes,
    decimal? distanceKm);

    Task<PriceResponse> StopPriceCalculator(string pickUpAdress, string dropoffAdress, decimal? durationMinutes, decimal? distanceKm, decimal zonePrice);

    Task<PriceResponse> CalculateTotalPriceAsync(
    PriceCalculationRequestDto Dto);
}
