using System;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class Address
{
    [Key]
    public int Id;
    public string firstLine { get; set; } = string.Empty;
    public string? secondLine {get;set;}

    public int postalCode {get;set;} = -1;

    public string? city {get;set;}

    public string? state {get;set;}

    public string country {get;set;} = string.Empty;

}
