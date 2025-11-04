using PegasusBackend.Configurations;
using PegasusBackend.Helpers.BookingHelpers;
using PegasusBackend.Models;

public class BookingFilterRequestForAdminDto
{
    public string? Search { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }

    public BookingStatus? Status { get; set; }
    public bool? DriverAssigned { get; set; }

    public DateOnly? Date { get; set; }
    public int? Month { get; set; }
    public int? Year { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public BookingPeriodHelper? Period { get; set; }

    public string? SortBy { get; set; } = string.Empty;
    public SortOrder SortOrder { get; set; } = SortOrder.Desc;

    public string? PickupAddress { get; set; }
    public string? DropoffAddress { get; set; }
    public string? FlightNumber { get; set; }

    public string? CustomerName { get; set; }
    public string? DriverName { get; set; }

    public int? HoursUntilPickup { get; set; }
}
