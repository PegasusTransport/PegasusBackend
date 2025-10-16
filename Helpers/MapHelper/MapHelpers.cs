using PegasusBackend.DTOs.MapDTOs;
using System.Globalization;
using System.Text.Json;

namespace PegasusBackend.Helpers
{
    public static class MapHelper
    {
        public static string ToCoord(decimal value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public static string BuildDirectionsUrl(List<CoordinateDto> coordinates, string apiKey)
        {
            if (coordinates == null || coordinates.Count < 2)
                throw new ArgumentException("At least 2 coordinates are required.");

            string origin = $"{ToCoord(coordinates.First().Latitude)},{ToCoord(coordinates.First().Longitude)}";
            string destination = $"{ToCoord(coordinates.Last().Latitude)},{ToCoord(coordinates.Last().Longitude)}";

            // Skapa waypoints (mellanliggande stopp)
            var waypoints = string.Join("|", coordinates
                .Skip(1)
                .Take(coordinates.Count - 2)
                .Select(c => $"{ToCoord(c.Latitude)},{ToCoord(c.Longitude)}"));

            // Bygg URL med svenska namn och optimerad väg
            var url = $"https://maps.googleapis.com/maps/api/directions/json?" +
                      $"origin={origin}" +
                      $"&destination={destination}" +
                      (!string.IsNullOrEmpty(waypoints) ? $"&waypoints={waypoints}" : string.Empty) +
                      $"&language=sv" +
                      $"&key={apiKey}";

            return url;
        }


        public static string BuildGeocodeUrl(CoordinateDto coord, string apiKey)
        {
            return $"https://maps.googleapis.com/maps/api/geocode/json?latlng={ToCoord(coord.Latitude)},{ToCoord(coord.Longitude)}&key={apiKey}";
        }

        public static RouteInfoDto ParseRouteInfo(string json)
        {
            using var document = JsonDocument.Parse(json);

            var legs = document.RootElement.GetProperty("routes")[0].GetProperty("legs");
            double totalDistance = 0, totalDuration = 0;
            var sectionList = new List<RouteSectionDto>();

            foreach (var s in legs.EnumerateArray())
            {
                var dist = s.GetProperty("distance").GetProperty("value").GetDouble();
                var dur = s.GetProperty("duration").GetProperty("value").GetDouble();

                totalDistance += dist;
                totalDuration += dur;

                sectionList.Add(new RouteSectionDto
                {
                    StartAddress = s.GetProperty("start_address").GetString(),
                    EndAddress = s.GetProperty("end_address").GetString(),
                    DistanceKm = (decimal)(dist / 1000),
                    DurationMinutes = (decimal)(dur / 60)
                });
            }

            return new RouteInfoDto
            {
                DistanceKm = (decimal)(totalDistance / 1000),
                DurationMinutes = (decimal)(totalDuration / 60),
                Sections = sectionList
            };
        }
    }
}
