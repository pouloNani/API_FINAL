using Core.Entities;

namespace Core.Interfaces;

public interface IPromotionRepository : IGenericRepository<Promotion>
{

    Task<IReadOnlyList<Promotion>> GetByShopIdAsync(int shopId);
    Task<Promotion?> GetPromotionWithProductsAsync(int id);

}