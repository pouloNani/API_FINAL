using Core.Entities;
using Core.Helpers;

namespace Core.Interfaces;

using Core.DTOs.Shop;

public interface IShopRepository : IGenericRepository<Shop>
{
    Task<PagedResult<ShopDto>> GetShopsAsync(ShopParams p);
    Task<PagedResult<ShopDto>> GetByOwnerIdAsync(string ownerId, PaginationParams p);
    Task<PagedResult<ShopDto>> GetOpenShopsNowAsync(ShopParams p);
    Task<ShopDto?> GetShopWithDetailsAsync(int id);
}