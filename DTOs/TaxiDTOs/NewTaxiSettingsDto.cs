using System.ComponentModel.DataAnnotations;

namespace PegasusBackend.DTOs.TaxiDTOs
{
    public class NewTaxiSettingsDTO
    {
        public decimal KmPrice { get; set; }

        public decimal MinutePrice { get; set; }

        public decimal StartPrice { get; set; }

        public decimal ZonePrice { get; set; }
    }
}
