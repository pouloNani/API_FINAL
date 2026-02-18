using System;
using System.Reflection.Metadata;

namespace Core.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal SellPrice { get; set; }

    public string UnitOfPrice {get;set;} = String.Empty;
    public string  CodeBar {get;set;} = string.Empty;

    public int Volume {get;set;}

    public string UnitOfVolume {get;set;} = string.Empty;

    public int Weight {get;set;}

    public string UnitOfWeight = string.Empty;

    public ICollection<PriceHistory> PriceHistories { get; set; } = 
        new List<PriceHistory>(); 

   public decimal BuyPrice {get;set;}
   public ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();
   public ICollection<Picture> Pictures { get; set; } = new List<Picture>();



    public int? AddressId { get; set; }
    public Address? Address { get; set; }

    public int ShopId { get; set; }
    public Shop Shop { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    
}