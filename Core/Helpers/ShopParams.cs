using Core.Entities;

namespace Core.Helpers;

public class ShopParams : PaginationParams
{
    public string? Name { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    public string? Category { get; set; }

    
}