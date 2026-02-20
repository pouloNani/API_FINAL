using System;

namespace Core.DTOs.Promotion;
using Core.Entities;

// Core/DTOs/Promotion/CreatePromotionDto.cs
public class CreatePromotionDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PromoType Type { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public int? BuyQuantity { get; set; }
    public int? GetQuantity { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public List<int> ProductIds { get; set; } = new(); // ← ajoute ça
}

// Core/DTOs/Promotion/UpdatePromotionDto.cs


public class UpdatePromotionDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public int? BuyQuantity { get; set; }
    public int? GetQuantity { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsActive { get; set; }
}

public class PromotionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PromoType Type { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public int? BuyQuantity { get; set; }
    public int? GetQuantity { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
}

public class UpdatePromoStrategyDto
{
    public PromoStrategy Strategy { get; set; }
}