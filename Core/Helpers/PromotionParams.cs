
namespace Core.Helpers;

public class PromotionParams : PaginationParams
{
    public int? ShopId { get; set; }
    public bool? IsActive { get; set; }
    public string? Type { get; set; }
}