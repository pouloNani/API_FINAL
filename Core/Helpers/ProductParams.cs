namespace Core.Helpers;
public enum ProductSortBy
{
    NameAsc,
    NameDesc,
    PriceAsc,
    PriceDesc,
    CreatedAtAsc,
    CreatedAtDesc
}

public class ProductParams : PaginationParams
{
    public string? Name { get; set; }
    public string? Barcode { get; set; }
    public int? ShopId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? HasActivePromotion { get; set; }
    public ProductSortBy SortBy { get; set; } = ProductSortBy.NameAsc;
}