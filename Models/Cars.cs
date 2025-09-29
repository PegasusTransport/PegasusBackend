using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusBackend.Models
{
    public class Cars
    {
        [Key]
        public int CarId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Make { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Model { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Type { get; set; }

        [Range(1, 8)] 
        public int Capacity { get; set; }

        [Required]
        [MaxLength(10)]
        public string LicensePlate { get; set; } = string.Empty;

        public virtual Drivers? Driver { get; set; }
    }
}
