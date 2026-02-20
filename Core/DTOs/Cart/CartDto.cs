using System;

namespace Core.DTOs.Cart;



public class CartDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalBeforeDiscount { get; set; }
    public List<CartItemDto> Items { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CartSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal TotalDiscount { get; set; }
    public int ItemCount { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CartItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int ShopId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal FinalPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal LineTotal { get; set; }
    public string? AppliedPromoName { get; set; }
    public string? AppliedPromoType { get; set; }
    public decimal? AppliedDiscountPercentage { get; set; }
    public int? AppliedBuyQuantity { get; set; }
    public int? AppliedGetQuantity { get; set; }
}

public class CreateCartDto
{
    public string Name { get; set; } = string.Empty;
}

public class RenameCartDto
{
    public string Name { get; set; } = string.Empty;
}

public class AddCartItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    public int? PromotionId { get; set; }
}

public class UpdateCartItemDto
{
    public int Quantity { get; set; }
}