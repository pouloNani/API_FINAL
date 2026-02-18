namespace Core.Entities;

public enum BillStatus
{
    Pending,
    Paid,
    Cancelled
}

public class Bill
{
    public int Id { get; set; }

    public string BillNumber { get; set; } = string.Empty;

    public DateTime BilledAt { get; set; } = DateTime.UtcNow;

    // Calculés automatiquement depuis les BillItems
    public decimal TotalAmount => BillItems.Sum(bi => bi.FinalPrice);
    public decimal TotalDiscount => BillItems.Sum(bi => bi.Discount);
    public decimal TotalBeforeDiscount => BillItems.Sum(bi => bi.UnitPrice * bi.Quantity);

    public BillStatus Status { get; set; } = BillStatus.Pending;

    // Relation avec Shop
    public int ShopId { get; set; }
    public Shop Shop { get; set; } = null!;

    // Relation avec BillItems
    public ICollection<BillItem> BillItems { get; set; } = new List<BillItem>();

    // Toutes les promos distinctes appliquées sur cette facture
    public IEnumerable<Promotion> AppliedPromotions =>
        BillItems
            .Where(bi => bi.Promotion != null)
            .Select(bi => bi.Promotion!)
            .DistinctBy(p => p.Id);

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}