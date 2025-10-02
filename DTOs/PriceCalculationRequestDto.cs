namespace PegasusBackend.DTOs
{
    public class PriceCalculationRequestDto
    {
        public string PickupAdress { get; set; }
        public string DropoffAdress { get; set; }
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
