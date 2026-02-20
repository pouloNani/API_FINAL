using Core.Entities;

namespace Core.Interfaces;

public interface IScheduleRepository : IGenericRepository<Schedule>
{
    Task<IReadOnlyList<Schedule>> GetByShopIdAsync(int shopId);
    Task<Schedule?> GetByShopAndDayAsync(int shopId, DayOfWeek day); 
    Task DeleteByShopIdAsync(int shopId);
    Task UpdatePerDayAsync(int shopId, DayOfWeek day, Schedule updated);
    
}