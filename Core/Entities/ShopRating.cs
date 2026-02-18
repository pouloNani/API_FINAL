using System;

namespace Core.Entities;



public class ShopRating
{
    public int Id { get; set; }
    
    public int ShopId { get; set; }
    public Shop Shop { get; set; } = null!;
    
    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;
    
    public int Score { get; set; } // 1 à 5
    public string Description { get; set; } = string.Empty;
    
    public ICollection<Picture> PicturesUrl { get; set; } = new List<Picture>();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}