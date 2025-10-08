using System.ComponentModel.DataAnnotations;

namespace PegasusBackend.DTOs.TaxiDTOs
{
    public class NewTaxiSettingsDTO
    {
        [Required(ErrorMessage = "KmPrice is required.")]
        [Range(0, 200, ErrorMessage = "KmPrice must be between 0 and 200 SEK per km.")]
        public decimal KmPrice { get; set; }

        [Required(ErrorMessage = "MinutePrice is required.")]
        [Range(0, 50, ErrorMessage = "MinutePrice must be between 0 and 50 SEK per minute.")]
        public decimal MinutePrice { get; set; }

        [Required(ErrorMessage = "StartPrice is required.")]
        [Range(0, 1000, ErrorMessage = "StartPrice must be between 0 and 1000 SEK.")]
        public decimal StartPrice { get; set; }

        [Required(ErrorMessage = "ZonePrice is required.")]
        [Range(0, 2000, ErrorMessage = "ZonePrice must be between 0 and 2000 SEK.")]
        public decimal ZonePrice { get; set; }
    }
}
