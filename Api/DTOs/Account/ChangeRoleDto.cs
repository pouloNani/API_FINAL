using System;
using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Account;

public class ChangeRoleDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;

}


