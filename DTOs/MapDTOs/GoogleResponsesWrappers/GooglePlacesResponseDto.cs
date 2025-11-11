using System.Text.Json.Serialization;

namespace PegasusBackend.DTOs.MapDTOs.GoogleResponses
{
    public class GooglePlacesResponseDto
    {
        [JsonPropertyName("suggestions")]
        public List<GoogleSuggestionDto> Suggestions { get; set; } = [];
    }

    public class GoogleSuggestionDto
    {
        [JsonPropertyName("placePrediction")]
        public GooglePlacePredictionDto PlacePrediction { get; set; } = null!;
    }

    public class GooglePlacePredictionDto
    {
        [JsonPropertyName("text")]
        public GoogleTextDto Text { get; set; } = null!;

        [JsonPropertyName("placeId")]  
        public string PlaceId { get; set; } = string.Empty;
    }

    public class GoogleTextDto
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }
}