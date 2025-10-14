using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.DTOs.MapDTOs;
using PegasusBackend.DTOs.TaxiDTOs;
using PegasusBackend.Models;
using PegasusBackend.Services.Interfaces.BookingInterfaces;
using System.Security.Cryptography;

namespace PegasusBackend.Services.Implementations.BookingServices
{
    public class BookingFactoryService : IBookingFactoryService
    {
        public Bookings CreateBookingEntity(
            CreateBookingDto bookingDto,
            RouteInfoDto routeInfo,
            decimal calculatedPrice,
            User? user,
            bool isGuestBooking)
        {
            return new Bookings
            {
                UserIdFk = user?.Id,
                GuestEmail = isGuestBooking ? bookingDto.Email : null,
                GuestFirstName = isGuestBooking ? bookingDto.FirstName : null,
                GuestLastName = isGuestBooking ? bookingDto.LastName : null,
                GuestPhoneNumber = isGuestBooking ? bookingDto.PhoneNumber : null,
                Price = calculatedPrice,
                BookingDateTime = DateTime.UtcNow,
                PickUpDateTime = bookingDto.PickUpDateTime,
                PickUpAdress = bookingDto.PickUpAddress,
                PickUpLatitude = bookingDto.PickUpLatitude,
                PickUpLongitude = bookingDto.PickUpLongitude,
                FirstStopAddress = bookingDto.FirstStopAddress,
                FirstStopLatitude = bookingDto.FirstStopLatitude,
                FirstStopLongitude = bookingDto.FirstStopLongitude,
                SecondStopAddress = bookingDto.SecondStopAddress,
                SecondStopLatitude = bookingDto.SecondStopLatitude,
                SecondStopLongitude = bookingDto.SecondStopLongitude,
                DropOffAdress = bookingDto.DropOffAddress,
                DropOffLatitude = bookingDto.DropOffLatitude,
                DropOffLongitude = bookingDto.DropOffLongitude,
                DistanceKm = routeInfo.DistanceKm,
                DurationMinutes = routeInfo.DurationMinutes,
                Flightnumber = bookingDto.Flightnumber,
                Comment = bookingDto.Comment,
                Status = isGuestBooking ? BookingStatus.PendingEmailConfirmation : BookingStatus.Confirmed,
                ConfirmationToken = isGuestBooking ? GenerateConfirmationToken() : null,
                ConfirmationTokenExpiresAt = isGuestBooking ? DateTime.UtcNow.AddHours(24) : null,
                IsConfirmed = !isGuestBooking,
                IsAvailable = !isGuestBooking
            };
        }

        public List<CoordinateDto> BuildCoordinatesList(CreateBookingDto dto)
        {
            var coordinates = new List<CoordinateDto>
            {
                new() { Latitude = (decimal)dto.PickUpLatitude, Longitude = (decimal)dto.PickUpLongitude }
            };

            if (!string.IsNullOrEmpty(dto.FirstStopAddress) && dto.FirstStopLatitude.HasValue && dto.FirstStopLongitude.HasValue)
            {
                coordinates.Add(new() { Latitude = (decimal)dto.FirstStopLatitude.Value, Longitude = (decimal)dto.FirstStopLongitude.Value });
            }

            if (!string.IsNullOrEmpty(dto.SecondStopAddress) && dto.SecondStopLatitude.HasValue && dto.SecondStopLongitude.HasValue)
            {
                coordinates.Add(new() { Latitude = (decimal)dto.SecondStopLatitude.Value, Longitude = (decimal)dto.SecondStopLongitude.Value });
            }

            coordinates.Add(new() { Latitude = (decimal)dto.DropOffLatitude, Longitude = (decimal)dto.DropOffLongitude });

            return coordinates;
        }

        public PriceCalculationRequestDto BuildPriceCalculationRequest(CreateBookingDto dto, RouteInfoDto routeInfo)
        {
            var request = new PriceCalculationRequestDto
            {
                PickupAdress = dto.PickUpAddress,
                DropoffAdress = dto.DropOffAddress,
                LastDistanceKm = 0,
                LastDurationMinutes = 0
            };

            if (routeInfo.Sections == null || routeInfo.Sections.Count == 0)
                return request;

            if (routeInfo.Sections.Count == 1)
            {
                request.LastDistanceKm = routeInfo.Sections[0].DistanceKm;
                request.LastDurationMinutes = routeInfo.Sections[0].DurationMinutes;
            }
            else if (routeInfo.Sections.Count == 2)
            {
                request.FirstStopAdress = dto.FirstStopAddress;
                request.FirstStopDistanceKm = routeInfo.Sections[0].DistanceKm;
                request.FirstStopDurationMinutes = routeInfo.Sections[0].DurationMinutes;
                request.LastDistanceKm = routeInfo.Sections[1].DistanceKm;
                request.LastDurationMinutes = routeInfo.Sections[1].DurationMinutes;
            }
            else if (routeInfo.Sections.Count >= 3)
            {
                request.FirstStopAdress = dto.FirstStopAddress;
                request.FirstStopDistanceKm = routeInfo.Sections[0].DistanceKm;
                request.FirstStopDurationMinutes = routeInfo.Sections[0].DurationMinutes;
                request.SecondStopAdress = dto.SecondStopAddress;
                request.SecondStopDistanceKm = routeInfo.Sections[1].DistanceKm;
                request.SecondStopDurationMinutes = routeInfo.Sections[1].DurationMinutes;
                request.LastDistanceKm = routeInfo.Sections[2].DistanceKm;
                request.LastDurationMinutes = routeInfo.Sections[2].DurationMinutes;
            }

            return request;
        }

        private string GenerateConfirmationToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber)
                .Replace("+", "")
                .Replace("/", "")
                .Replace("=", "");
        }
    }
}