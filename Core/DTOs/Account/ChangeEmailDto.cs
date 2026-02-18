using System;
using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Account;

public class ChangeEmailDto
{

    [Required]
    public string newEmail { get; set; } = string.Empty;

    [Required]
    public string password { get; set; } = string.Empty;



}
