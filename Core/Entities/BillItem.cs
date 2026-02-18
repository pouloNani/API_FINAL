namespace Core.Entities;

public class BillItem
{
    public int Id { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }          // Prix original au moment de la facturation

    public decimal FinalPrice { get; set; }          // Prix après application de la promo

    public decimal Discount => (UnitPrice * Quantity) - FinalPrice;  // Montant économisé

    // Relation avec Bill
    public int BillId { get; set; }
    public Bill Bill { get; set; } = null!;

    // Relation avec Product
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    // Promo appliquée (nullable, car un item peut ne pas avoir de promo)
    public int? PromotionId { get; set; }
    public Promotion? Promotion { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}