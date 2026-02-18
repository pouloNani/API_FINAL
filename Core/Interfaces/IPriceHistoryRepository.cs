using Core.Entities;

namespace Core.Interfaces;

public interface IPriceHistoryRepository : IGenericRepository<PriceHistory>
{
    Task<IReadOnlyList<PriceHistory>> GetByProductIdAsync(int productId);
    Task<PriceHistory?> GetLatestByProductIdAsync(int productId);
    Task<IReadOnlyList<PriceHistory>> GetByDateRangeAsync(int productId, DateTime from, DateTime to);
}