namespace Core.Helpers;
using Core.Entities;
public static class PromoResolver
{
    public static (Promotion? promo, decimal finalPrice) Resolve(
        Product product, 
        int quantity, 
        PromoStrategy strategy)
    {
        var activePromos = product.Promotions
            .Where(p => p.IsActive && 
                        DateTime.UtcNow >= p.StartDate && 
                        DateTime.UtcNow <= p.EndDate)
            .ToList();

        if (!activePromos.Any())
            return (null, product.SellPrice * quantity);

        return strategy switch
        {
            PromoStrategy.BestForClient => activePromos
                .Select(p => (promo: p, price: p.GetFinalPrice(product.SellPrice, quantity)))
                .OrderBy(x => x.price)
                .Select(x => ((Promotion?)x.promo, x.price))
                .First(),

            PromoStrategy.MostRecent => activePromos
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => ((Promotion?)p, p.GetFinalPrice(product.SellPrice, quantity)))
                .First(),

            PromoStrategy.FirstStarted => activePromos
                .OrderBy(p => p.StartDate)
                .Select(p => ((Promotion?)p, p.GetFinalPrice(product.SellPrice, quantity)))
                .First(),

            PromoStrategy.Cumulative => (
                null,
                activePromos.Aggregate(
                    product.SellPrice * quantity,
                    (price, promo) => promo.GetFinalPrice(price / quantity, quantity))
            ),

            _ => (null, product.SellPrice * quantity)
        };
    }
}