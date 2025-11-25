using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.Models;
using System.Globalization;

namespace PegasusBackend.Helpers.BookingHelpers
{
    public static class BookingMailHelper
    {
        public static string FormatStops(string? firstStop, string? secondStop)
        {
            var stops = new List<string>();

            if (!string.IsNullOrWhiteSpace(firstStop))
                stops.Add(firstStop);

            if (!string.IsNullOrWhiteSpace(secondStop))
                stops.Add(secondStop);

            return stops.Count > 0 ? string.Join(" → ", stops) : "No stops given!";
        }



        public static string FormatDateTime(DateTime dateTime)
        {
            return dateTime.ToString("dd MMMM yyyy HH:mm", new CultureInfo("sv-SE"));
        }
    }
}
