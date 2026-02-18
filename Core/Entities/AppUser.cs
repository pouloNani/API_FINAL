using System;
using Microsoft.AspNetCore.Identity;

namespace Core.Entities;

public class AppUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public Address? Address {get;set;}

    public UserRole Role {get;set;} 

    public ICollection<Shop>? Shops { get; set; }

    public ICollection<ShopRating> Ratings { get; set; } = new List<ShopRating>();


}

public enum UserRole
{
    client,
    owner,
    admin
}