using PegasusBackend.Configurations;
using PegasusBackend.Models;

namespace PegasusBackend.DTOs.BookingDTOs
{
    public class BookingSearchRequestDto
    {
        public string? Search { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public string? SortBy { get; set; } = string.Empty;
        public SortOrder SortOrder { get; set; } = SortOrder.Desc;
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public BookingStatus? Status { get; set; } // PendingEmailConfirm / Confirmed / Cancelled / Completed
        public bool? UpcomingOnly { get; set; }
    }
}
