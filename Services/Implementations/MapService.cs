using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using PegasusBackend.DTOs.MapDTOs;
using PegasusBackend.Models;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;
using System;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace PegasusBackend.Services.Implementations
{
    public class MapService(IConfiguration config) : IMapService
    {
        private readonly HttpClient _httpClient = new();
        private readonly string _key = config["GoogleMaps:ApiKey"]!;

        
        public async Task<ServiceResponse<RouteInfoDto>> GetRouteInfoAsync(List<CoordinateDto> coordinates)
        {
            if (coordinates == null || coordinates.Count < 2)
                return ServiceResponse<RouteInfoDto>.FailResponse(
                    HttpStatusCode.BadRequest,
                    "At least 2 coordinates are requared."
                );

            string ToCoord(decimal value) => value.ToString(CultureInfo.InvariantCulture);
            var pickupAdress = $"{ToCoord(coordinates.First().Latitude)},{ToCoord(coordinates.First().Longitude)}";
            var destination = $"{ToCoord(coordinates.Last().Latitude)},{ToCoord(coordinates.Last().Longitude)}";

            // Tar bort pickupAdress och destinationen. bhåller b-c 
            var stopps = string.Join("|", coordinates
                .Skip(1)
                .Take(coordinates.Count - 2)
                .Select(c => $"{ToCoord(c.Latitude)},{ToCoord(c.Longitude)}"));

            var url = $"https://maps.googleapis.com/maps/api/directions/json?origin={pickupAdress}&destination={destination}&key={_key}";
            if (!string.IsNullOrEmpty(stopps))
                url += $"&waypoints={stopps}";

            var result = await _httpClient.GetAsync(url);

            if (!result.IsSuccessStatusCode)
            {
                return ServiceResponse<RouteInfoDto>.FailResponse(
                    HttpStatusCode.BadRequest,
                    "Couldnt get ruttDestination from google api. Check Key and network."
                );
            }

            // Tolka datan som text först
            var json = await result.Content.ReadAsStringAsync();

            // och sen tolkar som JsonDocument. kan plocka ut datan med GetProperty. Tuple liknande med "status"
            using var document = JsonDocument.Parse(json);

            if (document.RootElement.TryGetProperty("status", out var apiStatus) && apiStatus.GetString() != "OK")
            {
                var messange = document.RootElement.TryGetProperty("error_message", out var errorMessage)
                    ? errorMessage.GetString()
                    : "Directions API error.";

                return ServiceResponse<RouteInfoDto>.FailResponse(
                    HttpStatusCode.BadRequest,
                    $"{apiStatus.GetString()}: {messange}"
                );
            }

            if (!document.RootElement.TryGetProperty("routes", out var routes) || routes.GetArrayLength() == 0)
            {
                return ServiceResponse<RouteInfoDto>.FailResponse(
                    HttpStatusCode.NotFound,
                    "There was no rutt/way found. Please try again later. "
                );
            }

            var sections = routes[0].GetProperty("legs");

            var sectionList = new List<RouteSectionDto>();

            double totalDistance = 0, totalDuration = 0;

            foreach (var section in sections.EnumerateArray())
            {
                var distanceValue = section.GetProperty("distance").GetProperty("value").GetDouble(); // får ut i meter
                var durationValue = section.GetProperty("duration").GetProperty("value").GetDouble(); // får ut isekunder

                totalDistance += distanceValue;
                totalDuration += durationValue;

                var sectionDto = new RouteSectionDto
                {
                    StartAddress = section.GetProperty("start_address").GetString(),
                    EndAddress = section.GetProperty("end_address").GetString(),
                    DistanceKm = (decimal)(distanceValue / 1000.0),
                    DurationMinutes = (decimal)(durationValue / 60.0)
                };

                sectionList.Add(sectionDto);
            }

            var routeInfo = new RouteInfoDto
            {
                DistanceKm = (decimal)(totalDistance / 1000.0),
                DurationMinutes = (decimal)(totalDuration / 60.0),
                Sections = sectionList
            };

            return ServiceResponse<RouteInfoDto>.SuccessResponse(
                HttpStatusCode.OK,
                routeInfo,
                "Ruttdata hämtad."
            );
        }

        // problem med att få Kommun för varje adress... Den är null även fast Kommun finns med i formaterad adress!
        public async Task<ServiceResponse<LocationInfoDto>> GetLocationDetailsAsync(CoordinateDto coordinateDto)
        {
            if (coordinateDto == null)
                return ServiceResponse<LocationInfoDto>.FailResponse(
                    HttpStatusCode.BadRequest,
                    "Coordinate data is missing."
                );

            string ToCoord(decimal value) => value.ToString(CultureInfo.InvariantCulture);

            var url = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={ToCoord(coordinateDto.Latitude)},{ToCoord(coordinateDto.Longitude)}&key={_key}";

            var result = await _httpClient.GetAsync(url);

            if (!result.IsSuccessStatusCode)
            {
                return ServiceResponse<LocationInfoDto>.FailResponse(
                    HttpStatusCode.BadRequest,
                    "Couldnt get location details from google api. Check Key and network."
                );
            }

            var json = await result.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(json);

            if (document.RootElement.TryGetProperty("status", out var apiStatus) && apiStatus.GetString() != "OK")
            {
                var messange = document.RootElement.TryGetProperty("error_message", out var errorMessage)
                    ? errorMessage.GetString()
                    : "Geocoding API error.";

                return ServiceResponse<LocationInfoDto>.FailResponse(
                    HttpStatusCode.BadRequest,
                    $"{apiStatus.GetString()}: {messange}"
                );
            }

            if (!document.RootElement.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
            {
                return ServiceResponse<LocationInfoDto>.FailResponse(
                    HttpStatusCode.NotFound,
                    "No address found for the provided coordinates."
                );
            }

            // Hämtar första resultatet (det mest relevanta i googles svar)
            var firstResult = results[0];

            var formattedAddress = firstResult.GetProperty("formatted_address").GetString();

            string? city = null;
            string? municipality = null;
            string? postalCode = null;

            if (firstResult.TryGetProperty("address_components", out var components))
            {
                foreach (var component in components.EnumerateArray())
                {
                    if (!component.TryGetProperty("types", out var types)) continue;

                    var typeList = types.EnumerateArray().Select(t => t.GetString()).ToList();

                    if (typeList.Contains("locality")) // Stad
                        city = component.GetProperty("long_name").GetString();

                    if (typeList.Contains("administrative_area_level_2")) // Kommun
                        municipality = component.GetProperty("long_name").GetString();

                    if (typeList.Contains("postal_code")) // Postnummer
                        postalCode = component.GetProperty("long_name").GetString();
                }
            }


            // Skapar DTO
            var locationInfo = new LocationInfoDto
            {
                FormattedAddress = formattedAddress,
                City = city,
                Municipality = municipality,
                PostalCode = postalCode,
                Latitude = coordinateDto.Latitude,
                Longitude = coordinateDto.Longitude
            };

            return ServiceResponse<LocationInfoDto>.SuccessResponse(
                HttpStatusCode.OK,
                locationInfo,
                "Location details successfully retrieved."
            );
        }

        /*
         * 
         * {
         * "results": [
                {
                  "formatted_address": "Arlanda Terminal 5, 190 45 Stockholm-Arlanda, Sverige",
                  "address_components": [
                    { "long_name": "190 45", "types": ["postal_code"] },
                    { "long_name": "Stockholm-Arlanda", "types": ["locality"] },
                    { "long_name": "Sigtuna kommun", "types": ["administrative_area_level_2"] },
                    { "long_name": "Stockholms län", "types": ["administrative_area_level_1"] },
                    { "long_name": "Sverige", "types": ["country", "political"] }
                  ]
                }
              ],
              "status": "OK"
            }
        */

    }
}