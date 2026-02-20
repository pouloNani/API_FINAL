using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PromotionRepository(StoreContext context) : GenericRepository<Promotion>(context), IPromotionRepository
{
    private readonly StoreContext _context = context;

    public async Task<IReadOnlyList<Promotion>> GetByShopIdAsync(int shopId) =>
    await _context.Promotions
        .Where(p => p.ShopId == shopId)
        .ToListAsync();

    public async Task<Promotion?> GetPromotionWithProductsAsync(int id) =>
        await _context.Promotions
            .Include(p => p.Products)
            .FirstOrDefaultAsync(p => p.Id == id);
            
    }