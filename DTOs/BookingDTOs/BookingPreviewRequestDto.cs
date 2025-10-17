namespace PegasusBackend.DTOs.BookingDTOs
{
    public class BookingPreviewRequestDto
    {
        // Pickup
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

        // Dropoff
        public string DropOffAddress { get; set; } = null!;
        public double DropOffLatitude { get; set; }
        public double DropOffLongitude { get; set; }

        // Optional
        public string? Flightnumber { get; set; }
    }
}

