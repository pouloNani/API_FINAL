using System;
using Core.Entities;

namespace Api.DTOs.Account;

public class UpdateUserDto
{
    public string? Email { get; set; }          
    public string? FirstName { get; set; }      
    public string? LastName { get; set; }      
    public string? PhoneNumber { get; set; }    
    public Address? Address { get; set; }        

    // facultatif pour admin uniquement
    public string? TargetEmail { get; set; }    
}
