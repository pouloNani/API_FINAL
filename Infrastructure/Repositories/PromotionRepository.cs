using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PromotionRepository(StoreContext context) : GenericRepository<Promotion>(context), IPromotionRepository
{
    private readonly StoreContext _context = context;

    public async Task<IReadOnlyList<Promotion>> GetByProductIdAsync(int productId) =>
        await _context.Promotions
            .Include(p => p.Products)
            .Where(p => p.Products.Any(pr => pr.Id == productId))
            .ToListAsync();

    public async Task<IReadOnlyList<Promotion>> GetActivePromotionsAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.Promotions
            .Include(p => p.Products)
            .Where(p => p.IsActive &&
                        p.StartDate <= now &&
                        p.EndDate >= now)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Promotion>> GetByTypeAsync(PromoType type) =>
        await _context.Promotions
            .Include(p => p.Products)
            .Where(p => p.Type == type)
            .ToListAsync();

    public async Task<IReadOnlyList<Promotion>> GetExpiredPromotionsAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.Promotions
            .Include(p => p.Products)
            .Where(p => p.EndDate < now)
            .ToListAsync();
    }
}