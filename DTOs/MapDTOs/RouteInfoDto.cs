using System.ComponentModel.DataAnnotations;

namespace PegasusBackend.DTOs.MapDTOs
{
    public class RouteInfoDto
    {
        [Required(ErrorMessage = "DistanceKm is required.")]
        [Range(0.1, 1000, ErrorMessage = "DistanceKm must be between 0.1 and 1000 km.")]
        public decimal DistanceKm { get; set; }

        [Required(ErrorMessage = "DurationMinutes is required.")]
        [Range(1, 600, ErrorMessage = "DurationMinutes must be between 1 and 600 minutes.")]
        public decimal DurationMinutes { get; set; }

        [Required(ErrorMessage = "Sections are required.")]
        [MinLength(1, ErrorMessage = "At least one section is required.")]
        public List<RouteSectionDto>? Sections { get; set; }
    }
}
