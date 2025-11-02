namespace PegasusBackend.DTOs.BookingDTOs
{
    public class UpdateBookingDto
    {
        // Identifies which booking to update
        public int BookingId { get; set; }
        public DateTime PickUpDateTime { get; set; }
        public string PickUpAddress { get; set; } = null!;
        public double PickUpLatitude { get; set; }
        public double PickUpLongitude { get; set; }
        public string? FirstStopAddress { get; set; }
        public double? FirstStopLatitude { get; set; }
        public double? FirstStopLongitude { get; set; }

        public string? SecondStopAddress { get; set; }
        public double? SecondStopLatitude { get; set; }
        public double? SecondStopLongitude { get; set; }

        public string DropOffAddress { get; set; } = null!;
        public double DropOffLatitude { get; set; }
        public double DropOffLongitude { get; set; }

        public string? Flightnumber { get; set; }
        public string? Comment { get; set; }
    }
}
