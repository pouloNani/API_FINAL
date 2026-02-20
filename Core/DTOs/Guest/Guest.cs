using System;

namespace Core.DTOs.Guest;


    public class GuestCheckoutDto
{
    public string GuestEmail { get; set; } = string.Empty;
    public string GuestFirstName { get; set; } = string.Empty;
    public string GuestLastName { get; set; } = string.Empty;
    public string GuestPhone { get; set; } = string.Empty;

    // Adresse
    public string GuestStreet { get; set; } = string.Empty;
    public string GuestCity { get; set; } = string.Empty;
    public string GuestZipCode { get; set; } = string.Empty;
    public string GuestCountry { get; set; } = string.Empty;
}
