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

    // Relation avec User (celui qui a validé le cart)
    public string? UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;

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


    // Pour Guest User

    public string? GuestId { get; set; }

    public string? GuestEmail { get; set; }
    public string? GuestFirstName { get; set; }
    public string? GuestLastName { get; set; }
    public string? GuestPhone { get; set; }

    // Adresse 
    public string? GuestStreet { get; set; }
    public string? GuestCity { get; set; }
    public string? GuestZipCode { get; set; }
    public string? GuestCountry { get; set; }
}