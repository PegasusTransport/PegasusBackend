using System.ComponentModel.DataAnnotations;

namespace PegasusBackend.DTOs.TaxiDTOs
{
    public class PriceCalculationRequestDto
    {
        [Required(ErrorMessage = "PickupAdress is required.")]
        [StringLength(200, ErrorMessage = "PickupAdress can't exceed 200 characters.")]
        public string PickupAdress { get; set; } = null!;

        [Required(ErrorMessage = "DropoffAdress is required.")]
        [StringLength(200, ErrorMessage = "DropoffAdress can't exceed 200 characters.")]
        public string DropoffAdress { get; set; } = null!;

        [Required(ErrorMessage = "LastDistanceKm is required.")]
        [Range(0.1, 1000, ErrorMessage = "LastDistanceKm must be between 0.1 and 1000 km.")]
        public decimal LastDistanceKm { get; set; }

        [Required(ErrorMessage = "LastDurationMinutes is required.")]
        [Range(1, 600, ErrorMessage = "LastDurationMinutes must be between 1 and 600 minutes.")]
        public decimal LastDurationMinutes { get; set; }

        [StringLength(200, ErrorMessage = "FirstStopAdress can't exceed 200 characters.")]
        public string? FirstStopAdress { get; set; }

        [Range(0.1, 1000, ErrorMessage = "FirstStopDistanceKm must be between 0.1 and 1000 km.")]
        public decimal? FirstStopDistanceKm { get; set; }

        [Range(1, 600, ErrorMessage = "FirstStopDurationMinutes must be between 1 and 600 minutes.")]
        public decimal? FirstStopDurationMinutes { get; set; }

        [StringLength(200, ErrorMessage = "SecondStopAdress can't exceed 200 characters.")]
        public string? SecondStopAdress { get; set; }

        [Range(0.1, 1000, ErrorMessage = "SecondStopDistanceKm must be between 0.1 and 1000 km.")]
        public decimal? SecondStopDistanceKm { get; set; }

        [Range(1, 600, ErrorMessage = "SecondStopDurationMinutes must be between 1 and 600 minutes.")]
        public decimal? SecondStopDurationMinutes { get; set; }
    }
}
