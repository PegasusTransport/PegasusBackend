namespace PegasusBackend.DTOs.MapDTOs
{
    public class RouteInfoDto
    {
        public decimal DistanceKm { get; set; }
        public decimal DurationMinutes { get; set; }
        public List<RouteSectionDto>? Sections { get; set; }
    }
}
