using System.ComponentModel.DataAnnotations;

public class AdressDto
{
    [Required]
    public string firstLine { get; set; } = string.Empty;
    public string? secondLine {get;set;}

    [Required]
    public int postalCode {get;set;} = -1;

    [Required]
    public string? city {get;set;}

    [Required]
    public string? state {get;set;}

    [Required]
    public string country {get;set;} = string.Empty;
}