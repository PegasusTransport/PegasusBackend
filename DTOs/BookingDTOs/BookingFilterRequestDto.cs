using PegasusBackend.Helpers.BookingHelpers;

namespace PegasusBackend.DTOs.BookingDTOs
{
    public class BookingFilterRequestDto
    {
        public string? Status { get; set; }
        public bool? DriverAssigned { get; set; }
        public DateOnly? Date { get; set; }
        public int? Month { get; set; }
        public BookingPeriodHelper? Period { get; set; }
        public string? PickupAddress { get; set; }
        public string? DropoffAddress { get; set; }
        public string? FlightNumber { get; set; }
        public string? CustomerName { get; set; }
        public string? DriverName { get; set; }
        public int? HoursUntilPickup { get; set; }
        public bool? IsAvailable { get; set; }
    }
}
