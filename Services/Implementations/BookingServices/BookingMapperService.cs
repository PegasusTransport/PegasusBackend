using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.Models;
using PegasusBackend.Services.Interfaces.BookingInterfaces;

namespace PegasusBackend.Services.Implementations.BookingServices
{
    public class BookingMapperService : IBookingMapperService
    {
        public BookingResponseDto MapToResponseDTO(Bookings booking)
        {
            bool isGuestBooking = booking.UserIdFk == null;

            var email = isGuestBooking ? booking.GuestEmail ?? "" : booking.User?.Email ?? "unknown@unknown.com";
            var firstName = isGuestBooking ? booking.GuestFirstName ?? "" : booking.User?.FirstName ?? "";
            var lastName = isGuestBooking ? booking.GuestLastName ?? "" : booking.User?.LastName ?? "";
            var phoneNumber = isGuestBooking ? booking.GuestPhoneNumber ?? "" : booking.User?.PhoneNumber ?? "";

            var driver = booking.Driver;
            var driverUser = driver?.User;
            var driverCar = driver?.Car;

            string? driverName = null;
            if (driverUser != null)
                driverName = $"{driverUser.FirstName} {driverUser.LastName}".Trim();

            return new BookingResponseDto
            {
                BookingId = booking.BookingId,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                IsGuestBooking = isGuestBooking,
                Price = booking.Price,
                BookingDateTime = booking.BookingDateTime,
                PickUpDateTime = booking.PickUpDateTime,
                PickUpAddress = booking.PickUpAdress,
                PickUpLatitude = booking.PickUpLatitude,
                PickUpLongitude = booking.PickUpLongitude,
                FirstStopAddress = booking.FirstStopAddress,
                FirstStopLatitude = booking.FirstStopLatitude,
                FirstStopLongitude = booking.FirstStopLongitude,
                SecondStopAddress = booking.SecondStopAddress,
                SecondStopLatitude = booking.SecondStopLatitude,
                SecondStopLongitude = booking.SecondStopLongitude,
                DropOffAddress = booking.DropOffAdress,
                DropOffLatitude = booking.DropOffLatitude,
                DropOffLongitude = booking.DropOffLongitude,
                DistanceKm = booking.DistanceKm,
                DurationMinutes = booking.DurationMinutes,
                Flightnumber = booking.Flightnumber,
                Comment = booking.Comment,
                Status = booking.Status,
                IsConfirmed = booking.IsConfirmed,
                DriverId = booking.DriverIdFK,
                DriverName = booking.Driver != null
                ? $"{booking.Driver.User.FirstName} {booking.Driver.User.LastName}" : null,
                DriverProfilePicture = booking.Driver?.ProfilePicture,
                DriverCarMake = booking.Driver?.Car.Make,
                DriverCarModel = booking.Driver?.Car.Model,
                DriverCarLicensePlate = booking.Driver?.Car.LicensePlate,
                DriverPhoneNumber = booking.Driver?.User.PhoneNumber

            };
        }

        public List<BookingResponseDto> MapToResponseDTOs(List<Bookings> bookings)
        {
            return bookings.Select(b => MapToResponseDTO(b)).ToList();
        }
    }
}