using System;

namespace Core.Models;



public class CartModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString()[..8].ToUpper();
    public string Name { get; set; } = string.Empty;
    public List<CartItemModel> Items { get; set; } = [];

    // Calculés depuis les items
    public decimal TotalAmount => Items.Sum(i => i.FinalPrice);
    public decimal TotalDiscount => Items.Sum(i => i.Discount);
    public decimal TotalBeforeDiscount => Items.Sum(i => i.UnitPrice * i.Quantity);

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class CartItemModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int ShopId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal FinalPrice { get; set; }
    public decimal Discount => (UnitPrice * Quantity) - FinalPrice;
    public decimal LineTotal => FinalPrice;

    // Snapshot promo
    public int? PromotionId { get; set; }
    public string? AppliedPromoName { get; set; }
    public string? AppliedPromoType { get; set; }
    public decimal? AppliedDiscountPercentage { get; set; }
    public int? AppliedBuyQuantity { get; set; }
    public int? AppliedGetQuantity { get; set; }
}