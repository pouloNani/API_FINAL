// GetUserDto.cs
using System.ComponentModel.DataAnnotations;

public class GetUserDto
{
    public string? UserId { get; set; }

    [EmailAddress]
    public string? Email { get; set; }
}