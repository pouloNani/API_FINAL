using Core.Entities;
using Core.Helpers;
using Core.DTOs.Product;

namespace Core.Interfaces;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<PagedResult<Product>> GetProductsAsync(ProductParams productParams);
    Task<Product?> GetProductWithDetailsAsync(int id);
    Task<IReadOnlyList<Product>> SearchByBarcodeAsync(string barcode);
    Task<IReadOnlyList<Product>> GetProductsWithActivePromotionsAsync(int shopId);
    Task<List<ProductWithDistanceDto>> SearchNearbyAsync(
    string query, double lat, double lng, double radiusKm = 5);
}