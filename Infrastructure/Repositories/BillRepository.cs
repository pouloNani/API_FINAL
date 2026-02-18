using Core.Entities;
using Core.Helpers;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class BillRepository(StoreContext context) : GenericRepository<Bill>(context), IBillRepository
{
    private readonly StoreContext _context = context;

    public async Task<PagedResult<Bill>> GetBillsAsync(BillParams p)
    {
        var query = _context.Bills
            .Include(b => b.BillItems)
                .ThenInclude(bi => bi.Product)
            .Include(b => b.BillItems)
                .ThenInclude(bi => bi.Promotion)
            .AsQueryable();

        if (p.ShopId.HasValue)
            query = query.Where(b => b.ShopId == p.ShopId.Value);

        if (p.Status.HasValue)
            query = query.Where(b => b.Status == p.Status.Value);

        if (p.From.HasValue)
            query = query.Where(b => b.BilledAt >= p.From.Value);

        if (p.To.HasValue)
            query = query.Where(b => b.BilledAt <= p.To.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(b => b.BilledAt)
            .Skip((p.PageIndex - 1) * p.PageSize)
            .Take(p.PageSize)
            .ToListAsync();

        return PagedResult<Bill>.Create(items, total, p);
    }

    public async Task<IReadOnlyList<Bill>> GetByShopIdAsync(int shopId) =>
        await _context.Bills
            .Include(b => b.BillItems)
                .ThenInclude(bi => bi.Product)
            .Include(b => b.BillItems)
                .ThenInclude(bi => bi.Promotion)
            .Where(b => b.ShopId == shopId)
            .OrderByDescending(b => b.BilledAt)
            .ToListAsync();

    public async Task<Bill?> GetBillWithDetailsAsync(int id) =>
        await _context.Bills
            .Include(b => b.BillItems)
                .ThenInclude(bi => bi.Product)
            .Include(b => b.BillItems)
                .ThenInclude(bi => bi.Promotion)
            .Include(b => b.Shop)
            .FirstOrDefaultAsync(b => b.Id == id);

    public async Task<IReadOnlyList<Bill>> GetByStatusAsync(int shopId, BillStatus status) =>
        await _context.Bills
            .Include(b => b.BillItems)
                .ThenInclude(bi => bi.Product)
            .Where(b => b.ShopId == shopId && b.Status == status)
            .OrderByDescending(b => b.BilledAt)
            .ToListAsync();
}