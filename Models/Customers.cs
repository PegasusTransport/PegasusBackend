using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusBackend.Models
{
    public class Customers
    {
        [Key]
        public Guid CustomerId { get; set; }

        [Required]
        public Guid UserIdFK { get; set; }

        [ForeignKey(nameof(UserIdFK))]
        [Required]
        public virtual Users User { get; set; } = null!;

        public virtual ICollection<Bookings> Bookings { get; set; } = new List<Bookings>();
    }
}
