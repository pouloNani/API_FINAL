using Core.Entities;
using Core.Helpers;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Core.DTOs.Shop;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace Infrastructure.Repositories;

public class ShopRepository(StoreContext context, IMapper mapper) : GenericRepository<Shop>(context), IShopRepository
{
    private readonly StoreContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<PagedResult<ShopDto>> GetShopsAsync(ShopParams p)
    {
        var query = _context.Shops
            .Include(s => s.Address)
            .Include(s => s.Owner)
            .AsQueryable();

        query = ApplyCommonFilters(query, p);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(s => s.Id)
            .Skip((p.PageIndex - 1) * p.PageSize)
            .Take(p.PageSize)
            .ProjectTo<ShopDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return PagedResult<ShopDto>.Create(items, total, p);
    }

    public async Task<PagedResult<ShopDto>> GetByOwnerIdAsync(string ownerId, PaginationParams p)
    {
        var query = _context.Shops
            .Include(s => s.Address)
            .Where(s => s.OwnerId == ownerId);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(s => s.Id)
            .Skip((p.PageIndex - 1) * p.PageSize)
            .Take(p.PageSize)
            .ProjectTo<ShopDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return PagedResult<ShopDto>.Create(items, total, p);
    }

    public async Task<ShopDto?> GetShopWithDetailsAsync(int id) =>
        await _context.Shops
            .Include(s => s.Address)
            .Include(s => s.Owner)
            .Include(s => s.DaysSchedule)
            .Include(s => s.Products)
            .Include(s => s.Promotions)
            .Where(s => s.Id == id)
            .ProjectTo<ShopDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

    public async Task<PagedResult<ShopDto>> GetOpenShopsNowAsync(ShopParams p)
    {
        var now = DateTime.UtcNow;
        var today = now.DayOfWeek;
        var currentTime = TimeOnly.FromDateTime(now);

        var query = _context.Shops
            .Include(s => s.DaysSchedule)
            .Include(s => s.Address)
            .Where(s => s.Status == ShopStatus.Open &&
                        s.DaysSchedule.Any(sc =>
                            sc.Day == today &&
                            sc.OpenTime <= currentTime &&
                            sc.CloseTime >= currentTime));

        query = ApplyCommonFilters(query, p);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(s => s.Id)
            .Skip((p.PageIndex - 1) * p.PageSize)
            .Take(p.PageSize)
            .ProjectTo<ShopDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return PagedResult<ShopDto>.Create(items, total, p);
    }                                                                                                                                                                                                                                        

    private static IQueryable<Shop> ApplyCommonFilters(IQueryable<Shop> query, ShopParams p)
    {
        if (!string.IsNullOrEmpty(p.Name))
            query = query.Where(s => s.Name.Contains(p.Name));
        if (!string.IsNullOrEmpty(p.City))
            query = query.Where(s => s.Address != null && s.Address.City!.Contains(p.City));
        if (!string.IsNullOrEmpty(p.Country))
            query = query.Where(s => s.Address != null && s.Address.Country.Contains(p.Country));
        if (!string.IsNullOrEmpty(p.Status) && Enum.TryParse<ShopStatus>(p.Status, out var status))
            query = query.Where(s => s.Status == status);
        if (!string.IsNullOrEmpty(p.Type) && Enum.TryParse<ShopType>(p.Type, out var type))
            query = query.Where(s => s.Type == type);
        

        return query;
    }
}