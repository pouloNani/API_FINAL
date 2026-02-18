using System;
using System.ComponentModel.DataAnnotations;

using Core.Entities;
using Core.DTOs.Address;
namespace Core.DTOs.Shop;

public class ShopDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string VatNumber { get; set; } = string.Empty;
    public ShopType Type { get; set; }
    public ShopStatus Status { get; set; }
    public PromoStrategy PromoStrategy { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public AddressDto? Address { get; set; }
}


public class CreateShopDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string VatNumber { get; set; } = string.Empty;

    [Required]
    public ShopType Type { get; set; }

    public ShopStatus Status { get; set; } = ShopStatus.NotDefined;
    public PromoStrategy PromoStrategy { get; set; } = PromoStrategy.BestForClient;

    [Required]
    public AddressDto Address { get; set; } = null!;
}

public class CreateShopForOwnerDto : CreateShopDto
{
    [Required]
    public string OwnerId { get; set; } = string.Empty;
}

public class UpdateShopDto
{
    public string? OwnerId { get; set; }

    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(50)]
    public string? VatNumber { get; set; }

    public ShopType? Type { get; set; }
    public ShopStatus? Status { get; set; }
    public PromoStrategy? PromoStrategy { get; set; }
    public AddressDto? Address { get; set; }
}