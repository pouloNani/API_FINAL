using System.ComponentModel.DataAnnotations;
using Core.DTOs.Promotion;
using Core.Entities;

namespace Core.DTOs.Product;

public class CreateProductDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal SellPrice { get; set; }
    public decimal BuyPrice { get; set; }
    public UnitOfPrice UnitOfPrice { get; set; } = UnitOfPrice.unit;
    public string CodeBar { get; set; } = string.Empty;
    public int Volume { get; set; }
    public UnitOfVolume? UnitOfVolume { get; set; }
    public int Weight { get; set; }
    public UnitOfWeight? UnitOfWeight { get; set; }
}

public class UpdateProductDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? SellPrice { get; set; }
    public decimal? BuyPrice { get; set; }
    public UnitOfPrice? UnitOfPrice { get; set; }
    public string? CodeBar { get; set; }
    public int? Volume { get; set; }
    public UnitOfVolume? UnitOfVolume { get; set; }
    public int? Weight { get; set; }
    public UnitOfWeight? UnitOfWeight { get; set; }
}

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal SellPrice { get; set; }
    public decimal BuyPrice { get; set; }
    public UnitOfPrice UnitOfPrice { get; set; }
    public string CodeBar { get; set; } = string.Empty;
    public int Volume { get; set; }
    public UnitOfVolume? UnitOfVolume { get; set; }
    public int Weight { get; set; }
    public UnitOfWeight? UnitOfWeight { get; set; }
    public int ShopId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<PromotionDto> Promotions { get; set; } = new List<PromotionDto>();
}



public class PriceHistoryDto
{
    public int Id { get; set; }
    public decimal SellPrice { get; set; }
    public decimal BuyPrice { get; set; }
    public string UnitOfPrice { get; set; } = string.Empty;
    public string? ChangeReason { get; set; }
    public DateTime ChangedAt { get; set; }
    public int ProductId { get; set; }
}

public class ProductWithDistanceDto
{
    public int        Id          { get; set; }
    public string     Name        { get; set; } = "";
    public decimal    SellPrice   { get; set; }
    public decimal    FinalPrice  { get; set; }
    public string?    UnitOfPrice { get; set; }
    public int        ShopId      { get; set; }
    public string?    ShopName    { get; set; }
    public ShopStatus ShopStatus  { get; set; }
    public bool       ShopIsOpen  { get; set; }
    public int        DistanceM   { get; set; }
}