using System.Text.Json.Serialization;

namespace PegasusBackend.DTOs.MapDTOs.GoogleResponses
{
    public class GooglePlaceLocationDetailsDto
    {
        [JsonPropertyName("location")]
        public GoogleLocationDto Location { get; set; } = null!;
    }
    public class GoogleLocationDto
    {
        [JsonPropertyName("latitude")]
        public decimal Lat { get; set; }

        [JsonPropertyName("longitude")]
        public decimal Lng { get; set; }
    }

}
