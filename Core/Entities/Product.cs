using System;
using System.Reflection.Metadata;

namespace Core.Entities;

public enum UnitOfPrice{ kg,mg,g,l,ml,unit }
public enum UnitOfWeight{ kg,mg,g}
public enum UnitOfVolume {l,ml,cl}
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal SellPrice { get; set; }

    public UnitOfPrice UnitOfPrice {get;set;} = UnitOfPrice.unit;
    public string  CodeBar {get;set;} = string.Empty;

    public int Volume {get;set;}

    public UnitOfVolume UnitOfVolume {get;set;} = UnitOfVolume.l;

    public int Weight {get;set;}

    public UnitOfWeight UnitOfWeight = UnitOfWeight.g;

    public ICollection<PriceHistory> PriceHistories { get; set; } = 
        new List<PriceHistory>(); 

   public decimal BuyPrice {get;set;}
   public ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();
   public ICollection<Picture> Pictures { get; set; } = new List<Picture>();



    public int? AddressId { get; set; }
  

    public int ShopId { get; set; }
    public Shop Shop { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    
}