using System;

namespace Core.Entities;

public enum ShopStatus { Open, Closed, ClosingSoon , NotDefined}
public enum ShopType { Online, Physical, Both }
public enum PromoStrategy
{
    BestForClient,    // La plus avantageuse pour le client
    MostRecent,       // La dernière créée
    FirstStarted,     // La première qui commence
    Cumulative        // Toutes s'accumulent
}
public class Shop
{

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string VatNumber { get; set; } = string.Empty;
    public ShopStatus Status { get; set; }
    public ShopType Type { get; set; }

    public int AddressId { get; set; }
    public Address? Address { get; set; }

    public PromoStrategy PromoStrategy { get; set; } = PromoStrategy.BestForClient; 
    
    public ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();

    public ICollection<Schedule> DaysSchedule { get; set; } = new List<Schedule>();
    public ICollection<Product> Products { get; set; } = new List<Product>();

    public ICollection<Bill> Bills { get; set; } = new List<Bill>();
    public string OwnerId { get; set; } = string.Empty;
    public AppUser Owner { get; set; } = null!;

    public ICollection<ShopRating> Ratings { get; set; } = new List<ShopRating>();

    public ICollection<Picture> Pictures { get; set; } = new List<Picture>();




    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


}
