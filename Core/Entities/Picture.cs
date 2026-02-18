using System;

namespace Core.Entities;

public class Picture
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Lien vers le parent (un seul sera renseigné)
    public int? ShopId { get; set; }
    public Shop? Shop { get; set; }

    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    public int? ShopRatingId { get; set; }
    public ShopRating? ShopRating { get; set; }
}