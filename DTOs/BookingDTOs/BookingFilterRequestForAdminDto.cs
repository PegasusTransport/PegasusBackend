using PegasusBackend.Configurations;
using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.Helpers.BookingHelpers;
using PegasusBackend.Models;

public class BookingFilterRequestForAdminDto : BookingSearchRequestDto
{
    public bool? DriverAssigned { get; set; }
    public DateOnly? Date { get; set; }
    public int? Month { get; set; }
    public int? Year { get; set; }
    public BookingPeriodHelper? Period { get; set; }
    public string? PickupAddress { get; set; }
    public string? DropoffAddress { get; set; }
    public string? FlightNumber { get; set; }
    public string? CustomerName { get; set; }
    public string? DriverName { get; set; }
    public int? HoursUntilPickup { get; set; }
}
