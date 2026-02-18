using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ScheduleRepository(StoreContext context) : GenericRepository<Schedule>(context), IScheduleRepository
{
    private readonly StoreContext _context = context;

    public async Task<IReadOnlyList<Schedule>> GetByShopIdAsync(int shopId) =>
        await _context.Schedules
            .Where(s => s.ShopId == shopId)
            .OrderBy(s => s.Day)
            .ToListAsync();

    public async Task<Schedule?> GetByShopAndDayAsync(int shopId, DayOfWeek day) =>
        await _context.Schedules
            .FirstOrDefaultAsync(s => s.ShopId == shopId && s.Day == day);

    public async Task DeleteByShopIdAsync(int shopId)
    {
        var schedules = await _context.Schedules
            .Where(s => s.ShopId == shopId)
            .ToListAsync();

        _context.Schedules.RemoveRange(schedules);
    }

    public async Task UpdatePerDayAsync(int shopId, DayOfWeek day, Schedule updated)
    {
        var existing = await _context.Schedules
            .FirstOrDefaultAsync(s => s.ShopId == shopId && s.Day == day);

        if (existing is null)
        {
            await _context.Schedules.AddAsync(updated);
        }
        else
        {
            existing.OpenTime  = updated.OpenTime;
            existing.CloseTime = updated.CloseTime;
            existing.IsClosed  = updated.IsClosed;
            existing.UpdatedAt = DateTime.UtcNow;
            _context.Schedules.Update(existing);
        }
    }
}