namespace Core.Entities;

public class PriceHistory
{
    public int Id { get; set; }

    public decimal SellPrice { get; set; }
    public decimal BuyPrice { get; set; }
    public string UnitOfPrice { get; set; } = string.Empty;

    public string? ChangeReason { get; set; } 

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    // FK Product
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}