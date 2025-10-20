using System.ComponentModel.DataAnnotations;

public class LocationInfoDto
{
    public string? FormattedAddress { get; set; }
    public string? City { get; set; }
    public string? Municipality { get; set; }
    public string? PostalCode { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}
