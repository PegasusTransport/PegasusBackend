using PegasusBackend.DTOs.BookingDTOs;
using System.Globalization;

namespace PegasusBackend.Helpers
{
    public static class BookingMailHelper
    {
        public static string FormatStops(CreateBookingDto dto)
        {
            var stops = new List<string>();

            if (!string.IsNullOrWhiteSpace(dto.FirstStopAddress))
                stops.Add(dto.FirstStopAddress);

            if (!string.IsNullOrWhiteSpace(dto.SecondStopAddress))
                stops.Add(dto.SecondStopAddress);

            return stops.Count > 0 ? string.Join(" → ", stops) : "Inga stopp angivna!";
        }

        public static string FormatDateTime(DateTime dateTime)
        {
            return dateTime.ToString("dd MMMM yyyy HH:mm", new CultureInfo("sv-SE"));
        }
    }
}
