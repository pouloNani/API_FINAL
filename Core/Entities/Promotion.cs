namespace Core.Entities;

public enum PromoType
{
    Percentage,
    ForXGetY
}
public class Promotion
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
    public bool IsActive { get; set; } = true;

    // Relation avec Shop (la promo appartient au shop)
    public int ShopId { get; set; }
    public Shop Shop { get; set; } = null!;

    // Many-to-Many avec Product
    public ICollection<Product> Products { get; set; } = new List<Product>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public decimal GetFinalPrice(decimal originalPrice, int quantity = 1)
    {
        if (!IsActive || DateTime.UtcNow < StartDate || DateTime.UtcNow > EndDate)
            return originalPrice * quantity;

        return Type switch
        {
            PromoType.Percentage => originalPrice * quantity * (1 - DiscountPercentage!.Value / 100),
            PromoType.ForXGetY   => ForXGetYPrice(originalPrice, quantity),
            _                    => originalPrice * quantity
        };
    }

    private decimal ForXGetYPrice(decimal originalPrice, int quantity)
    {
        if (BuyQuantity is null || GetQuantity is null)
            return originalPrice * quantity;

        int totalPerCycle = BuyQuantity.Value + GetQuantity.Value;
        int fullCycles    = quantity / totalPerCycle;
        int remainder     = quantity % totalPerCycle;
        int paidUnits     = (fullCycles * BuyQuantity.Value) + Math.Min(remainder, BuyQuantity.Value);

        return originalPrice * paidUnits;
    }
}