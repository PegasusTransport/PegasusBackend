using PegasusBackend.Models;

namespace PegasusBackend.DTOs.BookingDTOs
{
    public class BookingResponseDto
    {
        public int BookingId { get; set; }

        // Customer info (either from User or Guest fields)
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public bool IsGuestBooking { get; set; }

        public decimal Price { get; set; }
        public DateTime BookingDateTime { get; set; }
        public DateTime PickUpDateTime { get; set; }

        // Pickup
        public string PickUpAddress { get; set; } = null!;
        public double PickUpLatitude { get; set; }
        public double PickUpLongitude { get; set; }

        // Optional stops
        public string? FirstStopAddress { get; set; }
        public double? FirstStopLatitude { get; set; }
        public double? FirstStopLongitude { get; set; }

        public string? SecondStopAddress { get; set; }
        public double? SecondStopLatitude { get; set; }
        public double? SecondStopLongitude { get; set; }

        // Dropoff
        public string DropOffAddress { get; set; } = null!;
        public double DropOffLatitude { get; set; }
        public double DropOffLongitude { get; set; }

        public decimal DistanceKm { get; set; }
        public decimal DurationMinutes { get; set; }
        public string? Flightnumber { get; set; }
        public string? Comment { get; set; }
        public BookingStatus Status { get; set; }
        public bool IsConfirmed { get; set; }
        public Guid? DriverId { get; set; }
    }
}