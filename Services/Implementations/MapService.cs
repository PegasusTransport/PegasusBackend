using PegasusBackend.DTOs.MapDTOs;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;
using System.Globalization;
using System.Globalization;
using System.Net;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace PegasusBackend.Services.Implementations
{
    public class MapService(IConfiguration config) : IMapService
    {
        private readonly HttpClient _httpClient = new();
        private readonly string _key = config["GoogleMaps:ApiKey"]!;


        //public async Task<ServiceResponse<RouteInfoDto>> GetRouteInfoAsync(List<CoordinateDto> coordinates)
        //{
        //    if (coordinates == null || coordinates.Count < 2)
        //        return ServiceResponse<RouteInfoDto?>.FailResponse(
        //            HttpStatusCode.BadRequest,
        //            "At least two coordinates (pickup and destination) are required."
        //        );

        //    string ToCoord(decimal value) => value.ToString(CultureInfo.InvariantCulture);

        //    var pickupAdress = $"{ToCoord(coordinates.First().Latitude)},{ToCoord(coordinates.First().Longitude)}";
        //    var destination = $"{ToCoord(coordinates.Last().Latitude)},{ToCoord(coordinates.Last().Longitude)}";

        //    var stopps = string.Join("|", coordinates
        //        .Skip(1)
        //        .Take(coordinates.Count - 2)
        //        .Select(c => $"{ToCoord(c.Latitude)},{ToCoord(c.Longitude)}"));

        //    var url = $"https://maps.googleapis.com/maps/api/directions/json?origin={pickupAdress}&destination={destination}&key={_key}";
        //    if (!string.IsNullOrEmpty(stopps))
        //        url += $"&waypoints={stopps}";

        //    var res = await _httpClient.GetAsync(url);
        //    if (!res.IsSuccessStatusCode)
        //    {
        //        return ServiceResponse<RouteInfoDto?>.FailResponse(
        //            HttpStatusCode.BadRequest,
        //            "Kunde inte hämta ruttdata från Google API."
        //        );
        //    }

        //    var json = await res.Content.ReadAsStringAsync();
        //    using var doc = JsonDocument.Parse(json);

        //    if (doc.RootElement.TryGetProperty("status", out var apiStatus) && apiStatus.GetString() != "OK")
        //    {
        //        var msg = doc.RootElement.TryGetProperty("error_message", out var em)
        //            ? em.GetString()
        //            : "Directions API error.";
        //        return ServiceResponse<RouteInfoDto?>.FailResponse(HttpStatusCode.BadRequest, $"{apiStatus.GetString()}: {msg}");
        //    }

        //    if (!doc.RootElement.TryGetProperty("routes", out var routes) || routes.GetArrayLength() == 0)
        //    {
        //        return ServiceResponse<RouteInfoDto?>.FailResponse(
        //            HttpStatusCode.NotFound,
        //            "Ingen rutt hittades mellan punkterna."
        //        );
        //    }

        //    double distance = 0, duration = 0;
        //    var legs = routes[0].GetProperty("legs");

        //    foreach (var leg in legs.EnumerateArray())
        //    {
        //        distance += leg.GetProperty("distance").GetProperty("value").GetDouble();
        //        duration += leg.GetProperty("duration").GetProperty("value").GetDouble();
        //    }

        //    // 9️⃣ Skapa DTO
        //    var routeInfo = new RouteInfoDto
        //    {
        //        DistanceKm = (decimal)(distance / 1000.0),    
        //        DurationMinutes = (decimal)(duration / 60.0)     
        //    };

        //    return ServiceResponse<RouteInfoDto?>.SuccessResponse(
        //        HttpStatusCode.OK,
        //        routeInfo,
        //        "Ruttdata hämtad."
        //    );
        //}


        //public async Task<ServiceResponse<LocationInfoDto>> GetLocationDetailsAsync(CoordinateDto coordinateDto)
        //{

        //}


    }
}
