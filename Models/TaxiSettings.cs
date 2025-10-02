using System.ComponentModel.DataAnnotations;

namespace PegasusBackend.Models
{
    public class TaxiSettings
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(0, 1000)]
        public decimal KmPrice { get; set; }

        [Required]
        [Range(0, 1000)]
        public decimal MinutePrice { get; set; }

        [Required]
        [Range(0, 1000)]
        public decimal StartPrice { get; set; }

        [Required]
        [Range(0, 10000)]
        public decimal ZonePrice { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
