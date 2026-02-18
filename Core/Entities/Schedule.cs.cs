using System;

namespace Core.Entities;


public class Schedule
{
    public int Id { get; set; }
    public DayOfWeek Day { get; set; }
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
    public bool IsClosed { get; set; }

    public int ShopId { get; set; }
    public Shop Shop { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
}
