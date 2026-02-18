using Core.Entities;

namespace Core.Interfaces;

public interface IPromotionRepository : IGenericRepository<Promotion>
{
    Task<IReadOnlyList<Promotion>> GetByProductIdAsync(int productId);
    Task<IReadOnlyList<Promotion>> GetActivePromotionsAsync();
    Task<IReadOnlyList<Promotion>> GetByTypeAsync(PromoType type);
    Task<IReadOnlyList<Promotion>> GetExpiredPromotionsAsync();
}