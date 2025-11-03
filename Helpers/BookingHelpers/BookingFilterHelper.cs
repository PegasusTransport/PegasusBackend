using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.Helpers.BookingHelpers;
using PegasusBackend.Models;

public static class BookingFilterHelper
{
    public static IQueryable<Bookings> ApplyFilters(this IQueryable<Bookings> query, BookingFilterRequestDto filters)
    {
        if (filters == null)
            return query;

        query = ApplyStatusFilters(query, filters);
        query = ApplyTimeFilters(query, filters);
        query = ApplyTextFilters(query, filters);
        query = ApplyRelationFilters(query, filters);

        return query;
    }

    private static IQueryable<Bookings> ApplyStatusFilters(IQueryable<Bookings> query, BookingFilterRequestDto filters)
    {
        if (!string.IsNullOrWhiteSpace(filters.Status))
            query = query.Where(b => b.Status.ToString().ToLower() == filters.Status.ToLower());

        if (filters.DriverAssigned.HasValue)
            query = query.Where(b => (b.DriverIdFK != null) == filters.DriverAssigned.Value);

        if (filters.IsAvailable.HasValue)
            query = query.Where(b => b.IsAvailable == filters.IsAvailable.Value);

        return query;
    }

    private static IQueryable<Bookings> ApplyTimeFilters(IQueryable<Bookings> query, BookingFilterRequestDto filters)
    {
        var now = DateTime.UtcNow;

        if (filters.Date.HasValue)
        {
            var date = filters.Date.Value.ToDateTime(TimeOnly.MinValue).Date;
            return query.Where(b => b.PickUpDateTime.Date == date);
        }

        if (filters.Month.HasValue)
            return query.Where(b => b.PickUpDateTime.Month == filters.Month.Value);

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
                    var weekStart = now.Date.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
                    var weekEnd = weekStart.AddDays(7);
                    query = query.Where(b => b.PickUpDateTime >= weekStart && b.PickUpDateTime < weekEnd);
                    break;
                case BookingPeriodHelper.ThisMonth:
                    query = query.Where(b => b.PickUpDateTime.Month == now.Month && b.PickUpDateTime.Year == now.Year);
                    break;
            }
        }

        if (filters.HoursUntilPickup.HasValue)
            query = query.Where(b => (b.PickUpDateTime - now).TotalHours <= filters.HoursUntilPickup.Value);

        return query;
    }

    private static IQueryable<Bookings> ApplyTextFilters(IQueryable<Bookings> query, BookingFilterRequestDto filters)
    {
        if (!string.IsNullOrWhiteSpace(filters.PickupAddress))
            query = query.Where(b => b.PickUpAdress.ToLower().Contains(filters.PickupAddress.ToLower()));

        if (!string.IsNullOrWhiteSpace(filters.DropoffAddress))
            query = query.Where(b => b.DropOffAdress.ToLower().Contains(filters.DropoffAddress.ToLower()));

        if (!string.IsNullOrWhiteSpace(filters.FlightNumber))
            query = query.Where(b => b.Flightnumber.ToLower().Contains(filters.FlightNumber.ToLower()));

        return query;
    }

    private static IQueryable<Bookings> ApplyRelationFilters(IQueryable<Bookings> query, BookingFilterRequestDto filters)
    {
        if (!string.IsNullOrWhiteSpace(filters.CustomerName))
            query = query.Where(b =>
                (b.GuestFirstName + " " + b.GuestLastName).ToLower().Contains(filters.CustomerName.ToLower()));

        if (!string.IsNullOrWhiteSpace(filters.DriverName))
            query = query.Where(b => b.Driver != null &&
                (b.Driver.User.FirstName + " " + b.Driver.User.LastName).ToLower().Contains(filters.DriverName.ToLower()));

        return query;
    }
}
