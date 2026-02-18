using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Address;

public class AddressDto
{
    public string FirstLine { get; set; } = string.Empty;
    public string? SecondLine { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

