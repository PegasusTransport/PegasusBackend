using PegasusBackend.DTOs;

public interface IPriceService
{
    Task<(bool Success, decimal? Price, string Message)> TaxiMeterPrice(
    decimal? durationMinutes,
    decimal? distanceKm);

    Task<(bool Success, decimal? Price, string Message)> StopPriceCalculator(string pickUpAdress, string dropoffAdress, decimal? durationMinutes, decimal? distanceKm, decimal zonePrice);

    Task<(bool Success, decimal? Price, string Massange)> CalculateTotalPriceAsync(
    PriceCalculationRequestDto Dto);
}
