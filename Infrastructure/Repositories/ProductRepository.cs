using Core.Entities;
using Core.Helpers;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ProductRepository(StoreContext context) : GenericRepository<Product>(context), IProductRepository
{
    private readonly StoreContext _context = context;

    
    public async Task<PagedResult<Product>> GetProductsAsync(ProductParams p)
    {
        var now = DateTime.UtcNow;
        var query = _context.Products
            .Include(pr => pr.Promotions)
            .Include(pr => pr.PriceHistories)
            .AsQueryable();

        if (p.ShopId.HasValue)
            query = query.Where(pr => pr.ShopId == p.ShopId.Value);

        if (!string.IsNullOrEmpty(p.Search))
        
            query = query.Where(pr => pr.Name.ToLower().Contains(p.Search.ToLower()) || pr.CodeBar == p.Search);

        if (p.MinPrice.HasValue)
            query = query.Where(pr => pr.SellPrice >= p.MinPrice.Value);

        if (p.MaxPrice.HasValue)
            query = query.Where(pr => pr.SellPrice <= p.MaxPrice.Value);

        if (p.HasActivePromotion.HasValue && p.HasActivePromotion.Value)
            query = query.Where(pr => pr.Promotions.Any(promo =>
                promo.IsActive &&
                promo.StartDate <= now &&
                promo.EndDate >= now));

        query = p.SortBy switch
        {
            ProductSortBy.NameAsc       => query.OrderBy(pr => pr.Name),
            ProductSortBy.NameDesc      => query.OrderByDescending(pr => pr.Name),
            ProductSortBy.PriceAsc      => query.OrderBy(pr => pr.SellPrice),
            ProductSortBy.PriceDesc     => query.OrderByDescending(pr => pr.SellPrice),
            ProductSortBy.CreatedAtAsc  => query.OrderBy(pr => pr.CreatedAt),
            ProductSortBy.CreatedAtDesc => query.OrderByDescending(pr => pr.CreatedAt),
            _                           => query.OrderBy(pr => pr.Name)
        };

        var total = await query.CountAsync();
        var items = await query
            .Skip((p.PageIndex - 1) * p.PageSize)
            .Take(p.PageSize)
            .ToListAsync();

        return PagedResult<Product>.Create(items, total, p);
    }

    public async Task<Product?> GetProductWithDetailsAsync(int id) =>
        await _context.Products
            .Include(p => p.Promotions)
            .Include(p => p.PriceHistories)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IReadOnlyList<Product>> SearchByBarcodeAsync(string barcode) =>
        await _context.Products
            .Include(p => p.Promotions)
            .Where(p => p.CodeBar == barcode)
            .ToListAsync();

    public async Task<IReadOnlyList<Product>> GetProductsWithActivePromotionsAsync(int shopId)
    {
        var now = DateTime.UtcNow;
        return await _context.Products
            .Include(p => p.Promotions.Where(pr =>
                pr.IsActive &&
                pr.StartDate <= now &&
                pr.EndDate >= now))
            .Where(p => p.ShopId == shopId &&
                        p.Promotions.Any(pr =>
                            pr.IsActive &&
                            pr.StartDate <= now &&
                            pr.EndDate >= now))
            .ToListAsync();
    }
}