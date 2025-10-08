using PegasusBackend.DTOs.TaxiDTOs;
using PegasusBackend.Responses;

public interface IPriceService
{
    Task<ServiceResponse<decimal>> TaxiMeterPrice(
    decimal? durationMinutes,
    decimal? distanceKm);

    Task<ServiceResponse<decimal>> StopPriceCalculator(string pickUpAdress, string dropoffAdress, decimal? durationMinutes, decimal? distanceKm, decimal zonePrice);

    Task<ServiceResponse<decimal>> CalculateTotalPriceAsync(
    PriceCalculationRequestDto Dto);
}
