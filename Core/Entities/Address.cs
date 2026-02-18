using System;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class Address
{
    [Key]
    public int Id { get; set; }
    public string FirstLine { get; set; } = string.Empty;
    public string? SecondLine { get; set; }
    public int PostalCode { get; set; } = -1;
    public string? City { get; set; }
    public string? State { get; set; }
    public string Country { get; set; } = string.Empty;
}