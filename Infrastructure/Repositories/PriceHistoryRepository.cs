using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PriceHistoryRepository(StoreContext context) : GenericRepository<PriceHistory>(context), IPriceHistoryRepository
{
    private readonly StoreContext _context = context;

    public async Task<IReadOnlyList<PriceHistory>> GetByProductIdAsync(int productId) =>
        await _context.PriceHistories
            .Where(ph => ph.ProductId == productId)
            .OrderByDescending(ph => ph.ChangedAt)
            .ToListAsync();

    public async Task<PriceHistory?> GetLatestByProductIdAsync(int productId) =>
        await _context.PriceHistories
            .Where(ph => ph.ProductId == productId)
            .OrderByDescending(ph => ph.ChangedAt)
            .FirstOrDefaultAsync();

    public async Task<IReadOnlyList<PriceHistory>> GetByDateRangeAsync(int productId, DateTime from, DateTime to) =>
        await _context.PriceHistories
            .Where(ph => ph.ProductId == productId &&
                         ph.ChangedAt >= from &&
                         ph.ChangedAt <= to)
            .OrderByDescending(ph => ph.ChangedAt)
            .ToListAsync();
}