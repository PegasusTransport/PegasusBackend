using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.DTOs.MapDTOs;
using PegasusBackend.Responses;

namespace PegasusBackend.DTOs.ValidationResults
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public RouteInfoDto? RouteInfo { get; set; }
        public decimal CalculatedPrice { get; set; }
        public ServiceResponse<BookingResponseDto>? ErrorResponse { get; set; }
    }
}