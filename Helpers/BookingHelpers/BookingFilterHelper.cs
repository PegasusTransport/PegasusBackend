using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.Helpers.BookingHelpers;
using PegasusBackend.Models;

public static class BookingFilterHelper
{
    public static IQueryable<Bookings> ApplyFilters(this IQueryable<Bookings> query, BookingFilterRequestForAdminDto filters)
    {
        if (filters == null)
            return query;

        query = ApplyStatusFilters(query, filters);
        query = ApplyTimeFilters(query, filters);
        query = ApplyTextFilters(query, filters);
        query = ApplyRelationFilters(query, filters);

        return query;
    }

    private static IQueryable<Bookings> ApplyStatusFilters(IQueryable<Bookings> query, BookingFilterRequestForAdminDto filters)
    {
        if (filters.Status.HasValue)
            query = query.Where(b => b.Status == filters.Status.Value);

        if (filters.DriverAssigned.HasValue)
            query = query.Where(b => (b.DriverIdFK != null) == filters.DriverAssigned.Value);

        return query;
    }

    private static IQueryable<Bookings> ApplyTimeFilters(IQueryable<Bookings> query, BookingFilterRequestForAdminDto filters)
    {
        var now = DateTime.UtcNow;

        if (filters.Date.HasValue)
        {
            var date = filters.Date.Value.ToDateTime(TimeOnly.MinValue).Date;
            query = query.Where(b => b.PickUpDateTime.Date == date);
        }

        if (filters.FromDate.HasValue)
            query = query.Where(b => b.PickUpDateTime >= filters.FromDate.Value);

        if (filters.ToDate.HasValue)
            query = query.Where(b => b.PickUpDateTime <= filters.ToDate.Value);

        if (filters.Month.HasValue)
        {
            query = query.Where(b => b.PickUpDateTime.Month == filters.Month.Value);
            if (filters.Year.HasValue)
                query = query.Where(b => b.PickUpDateTime.Year == filters.Year.Value);
            else
                query = query.Where(b => b.PickUpDateTime.Year == now.Year);
        }

        if (filters.Period.HasValue)
        {
            switch (filters.Period.Value)
            {
                case BookingPeriodHelper.Past:
                    query = query.Where(b => b.PickUpDateTime < now);
                    break;
                case BookingPeriodHelper.Current:
                    query = query.Where(b => b.PickUpDateTime >= now && b.PickUpDateTime <= now.AddDays(3));
                    break;
                case BookingPeriodHelper.Future:
                    query = query.Where(b => b.PickUpDateTime > now.AddDays(3));
                    break;
                case BookingPeriodHelper.Today:
                    query = query.Where(b => b.PickUpDateTime.Date == now.Date);
                    break;
                case BookingPeriodHelper.ThisWeek:
                    // Start måndag i aktuell vecka (även korrekt för söndag)
                    int diff = (7 + (int)now.DayOfWeek - (int)DayOfWeek.Monday) % 7;
                    var weekStart = now.Date.AddDays(-diff);
                    var weekEnd = weekStart.AddDays(7);
                    query = query.Where(b => b.PickUpDateTime >= weekStart && b.PickUpDateTime < weekEnd);
                    break;
                case BookingPeriodHelper.ThisMonth:
                    query = query.Where(b => b.PickUpDateTime.Month == now.Month && b.PickUpDateTime.Year == now.Year);
                    break;
            }
        }

        if (filters.HoursUntilPickup.HasValue)
        {
            var end = now.AddHours(filters.HoursUntilPickup.Value);
            query = query.Where(b => b.PickUpDateTime >= now && b.PickUpDateTime <= end);
        }

        return query;
    }

    private static IQueryable<Bookings> ApplyTextFilters(IQueryable<Bookings> query, BookingFilterRequestForAdminDto filters)
    {
        if (!string.IsNullOrWhiteSpace(filters.PickupAddress))
            query = query.Where(b => b.PickUpAdress.ToLower().Contains(filters.PickupAddress.ToLower()));

        if (!string.IsNullOrWhiteSpace(filters.DropoffAddress))
            query = query.Where(b => b.DropOffAdress.ToLower().Contains(filters.DropoffAddress.ToLower()));

        if (!string.IsNullOrWhiteSpace(filters.FlightNumber))
            query = query.Where(b => b.Flightnumber.ToLower().Contains(filters.FlightNumber.ToLower()));

        return query;
    }

    private static IQueryable<Bookings> ApplyRelationFilters(IQueryable<Bookings> query, BookingFilterRequestForAdminDto filters)
    {

        if (!string.IsNullOrWhiteSpace(filters.CustomerName))
        {
            var term = filters.CustomerName.ToLower();
            query = query.Where(b =>
                ((b.GuestFirstName + " " + b.GuestLastName).ToLower().Contains(term)) ||
                (b.User != null && ((b.User.FirstName + " " + b.User.LastName).ToLower().Contains(term)))
            );
        }

        if (!string.IsNullOrWhiteSpace(filters.DriverName))
        {
            var term = filters.DriverName.ToLower();
            query = query.Where(b => b.Driver != null &&
                (b.Driver.User.FirstName + " " + b.Driver.User.LastName).ToLower().Contains(term));
        }

        return query;
    }

}
