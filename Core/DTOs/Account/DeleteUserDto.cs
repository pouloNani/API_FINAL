using System;
using System.ComponentModel.DataAnnotations;

namespace Api.DTOs;

public class DeleteUserDto
{

    public int UserId { get; set; }

    [EmailAddress]
    public string Email {get;set;}  = string.Empty;

}
