using PegasusBackend.Configurations;
using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.Models;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces.BookingInterfaces;
using System.Net;

namespace PegasusBackend.Helpers.BookingHelpers
{
    public class ValidateUpdateRuleHelper
    {
        private readonly IBookingValidationService _validationService;
        private readonly BookingRulesSettings _bookingRules;

        public ValidateUpdateRuleHelper(IBookingValidationService validationService, BookingRulesSettings bookingRulesSettings)
        {
            _validationService = validationService;
            _bookingRules = bookingRulesSettings;
        }

        public async Task<ServiceResponse<BookingResponseDto>?> ValidateUpdateRulesAsync(Bookings booking, UpdateBookingDto dto)
        {
            // How about if they change adresses? maybe the driver dosent have time for their next customer?
            if (booking.PickUpDateTime != dto.PickUpDateTime)
            {
                var result = await _validationService.ValidatePickupTimeAsync(dto.PickUpDateTime, _bookingRules.MinHoursBeforePickupForChange);
                if (result.StatusCode != HttpStatusCode.OK)
                    return ServiceResponse<BookingResponseDto>.FailResponse(HttpStatusCode.Forbidden, "It's too late to change your pickup time.");
            }

            return null;
        }
    }
}
