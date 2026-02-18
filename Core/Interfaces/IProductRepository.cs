using Core.Entities;
using Core.Helpers;

namespace Core.Interfaces;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<PagedResult<Product>> GetProductsAsync(ProductParams productParams);
    Task<Product?> GetProductWithDetailsAsync(int id);
    Task<IReadOnlyList<Product>> SearchByBarcodeAsync(string barcode);
    Task<IReadOnlyList<Product>> GetProductsWithActivePromotionsAsync(int shopId);
}