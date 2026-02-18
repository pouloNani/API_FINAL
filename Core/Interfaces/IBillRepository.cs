
using Core.Entities;
using Core.Helpers;

namespace Core.Interfaces;

public interface IBillRepository : IGenericRepository<Bill>
{
    Task<PagedResult<Bill>> GetBillsAsync(BillParams billParams);
    Task<IReadOnlyList<Bill>> GetByShopIdAsync(int shopId);
    Task<Bill?> GetBillWithDetailsAsync(int id);    // inclut BillItems, Promotions, Products
    Task<IReadOnlyList<Bill>> GetByStatusAsync(int shopId, BillStatus status);
}