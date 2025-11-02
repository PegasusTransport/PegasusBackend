using System.Text.Json.Serialization;

namespace PegasusBackend.Helpers.GoogleResponses
{
    public class GooglePlacesResponse
    {
        [JsonPropertyName("suggestions")]
        public List<GoogleSuggestion> Suggestions { get; set; } = [];
    }

    public class GoogleSuggestion
    {
        [JsonPropertyName("placePrediction")]
        public GooglePlacePrediction PlacePrediction { get; set; } = null!;
    }

    public class GooglePlacePrediction
    {
        [JsonPropertyName("text")]
        public GoogleText Text { get; set; } = null!;
    }

    public class GoogleText
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }
}
