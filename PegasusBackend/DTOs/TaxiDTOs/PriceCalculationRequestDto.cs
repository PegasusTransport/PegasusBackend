using System.ComponentModel.DataAnnotations;

namespace PegasusBackend.DTOs.TaxiDTOs
{
    public class PriceCalculationRequestDto
    {
        public string PickupAdress { get; set; } = null!;
        public string DropoffAdress { get; set; } = null!;
        public decimal LastDistanceKm { get; set; }
        public decimal LastDurationMinutes { get; set; }
        public string? FirstStopAdress { get; set; }
        public decimal? FirstStopDistanceKm { get; set; }
        public decimal? FirstStopDurationMinutes { get; set; }
        public string? SecondStopAdress { get; set; }
        public decimal? SecondStopDistanceKm { get; set; }
        public decimal? SecondStopDurationMinutes { get; set; }
    }
}
