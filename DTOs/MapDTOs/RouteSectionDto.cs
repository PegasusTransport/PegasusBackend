using System.ComponentModel.DataAnnotations;

namespace PegasusBackend.DTOs.MapDTOs
{
    public class RouteSectionDto
    {
        [Required(ErrorMessage = "StartAddress is required.")]
        [StringLength(200, ErrorMessage = "StartAddress can't exceed 200 characters.")]
        public string? StartAddress { get; set; }

        [Required(ErrorMessage = "EndAddress is required.")]
        [StringLength(200, ErrorMessage = "EndAddress can't exceed 200 characters.")]
        public string? EndAddress { get; set; }

        [Required(ErrorMessage = "DistanceKm is required.")]
        [Range(0.1, 1000, ErrorMessage = "DistanceKm must be between 0.1 and 1000 km.")]
        public decimal DistanceKm { get; set; }

        [Required(ErrorMessage = "DurationMinutes is required.")]
        [Range(1, 600, ErrorMessage = "DurationMinutes must be between 1 and 600 minutes.")]
        public decimal DurationMinutes { get; set; }
    }
}
