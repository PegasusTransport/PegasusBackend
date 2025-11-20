using PegasusBackend.DTOs.MapDTOs;

namespace PegasusBackend.DTOs.BookingDTOs
{
    public class BookingPreviewResponseDto
    {
        public decimal DistanceKm { get; set; }
        public decimal DurationMinutes { get; set; }
        public decimal Price { get; set; }
        public string Message { get; set; } = null!;

        // Reuses existing RouteSectionDto from MapDTOs
        public List<RouteSectionDto>? Sections { get; set; }
    }
}
