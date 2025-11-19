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

            string email, firstName, lastName, phoneNumber;

            if (isGuestBooking)
            {
                email = booking.GuestEmail!;
                firstName = booking.GuestFirstName!;
                lastName = booking.GuestLastName!;
                phoneNumber = booking.GuestPhoneNumber!;
            }
            else
            {
                email = booking.User?.Email ?? "unknown@unknown.com";
                firstName = booking.User?.FirstName ?? "";
                lastName = booking.User?.LastName ?? "";
                phoneNumber = booking.User?.PhoneNumber ?? "";
            }

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