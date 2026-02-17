using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Api.DTOs.Account;

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string email { get; set; } = string.Empty;

    [Required]
    public string password {get;set;} = string.Empty;

}


