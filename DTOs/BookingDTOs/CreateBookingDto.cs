using System.ComponentModel.DataAnnotations;

namespace PegasusBackend.DTOs.BookingDTOs
{
    public class CreateBookingDto
    {
        // Customer info - required for all bookings
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;

        // Pickup info (obligatorisk)
        public DateTime PickUpDateTime { get; set; }
        public string PickUpAddress { get; set; } = null!;
        public double PickUpLatitude { get; set; }
        public double PickUpLongitude { get; set; }

        // First stop (optional)
        public string? FirstStopAddress { get; set; }
        public double? FirstStopLatitude { get; set; }
        public double? FirstStopLongitude { get; set; }

        // Second stop (optional)
        public string? SecondStopAddress { get; set; }
        public double? SecondStopLatitude { get; set; }
        public double? SecondStopLongitude { get; set; }

        // Dropoff info (obligatorisk)
        public string DropOffAddress { get; set; } = null!;
        public double DropOffLatitude { get; set; }
        public double DropOffLongitude { get; set; }

        // Optional fields
        public string? Flightnumber { get; set; }
        public string? Comment { get; set; }

        // Price from frontend for verification
        public decimal ExpectedPrice { get; set; }
    }
}