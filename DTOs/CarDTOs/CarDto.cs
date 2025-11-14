using System.Text.Json.Serialization;

namespace PegasusBackend.DTOs.CarDTOs
{
    public class CarDto
    {
        public string RegNr { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        [JsonPropertyName("technical")]
        public TechnicalDto Capacity { get; set; } = new();
    }
    public class TechnicalDto
    {
        [JsonPropertyName("number_of_passengers")]
        public int NumberOfPassengers { get; set; }
    }
}