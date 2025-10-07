namespace PegasusBackend.DTOs.MapDTOs
{
    public class RouteSectionDto
    {
        public string? StartAddress { get; set; }
        public string? EndAddress { get; set; }
        public decimal DistanceKm { get; set; }
        public decimal DurationMinutes { get; set; }
        public decimal? EstimatedPrice { get; set; }
    }
}
