using System.ComponentModel.DataAnnotations;

public class LocationInfoDto
{
    [Required(ErrorMessage = "FormattedAddress is required.")]
    [StringLength(200, ErrorMessage = "FormattedAddress can't exceed 200 characters.")]
    public string? FormattedAddress { get; set; }

    [Required(ErrorMessage = "City is required.")]
    [StringLength(100, ErrorMessage = "City can't exceed 100 characters.")]
    public string? City { get; set; }

    [Required(ErrorMessage = "Municipality is required.")]
    [StringLength(100, ErrorMessage = "Municipality can't exceed 100 characters.")]
    public string? Municipality { get; set; }

    [Required(ErrorMessage = "PostalCode is required.")]
    [RegularExpression(@"^\d{3}\s?\d{2}$", ErrorMessage = "PostalCode must be in the format 12345 or 123 45.")]
    public string? PostalCode { get; set; }

    [Required(ErrorMessage = "Latitude is required.")]
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90.")]
    public decimal Latitude { get; set; }

    [Required(ErrorMessage = "Longitude is required.")]
    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180.")]
    public decimal Longitude { get; set; }
}
