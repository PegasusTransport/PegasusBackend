using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.Responses;

namespace PegasusBackend.DTOs.ValidationResults
{
    public class PriceValidationResult
    {
        public bool IsValid { get; set; }
        public decimal Price { get; set; }
        public ServiceResponse<BookingResponseDto>? ErrorResponse { get; set; }
    }
}