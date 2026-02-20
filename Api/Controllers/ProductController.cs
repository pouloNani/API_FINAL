using AutoMapper;
using Core.DTOs.Promotion;
using Core.DTOs.Product;
using Core.Entities;
using Core.Helpers;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;

[Route("products")]
public class ProductController(
    IProductRepository productRepository,
    IShopRepository shopRepository,
    IPromotionRepository promotionRepository,
    IMapper mapper) : BaseApiController
{
    private string? GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    // ───────────────────────────────────────────
    // LECTURE
    // ───────────────────────────────────────────

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts([FromQuery] ProductParams p)
    {
    
        var result = await productRepository.GetProductsAsync(p);
        return Ok(new PagedResult<ProductDto>
        {
            Data = mapper.Map<List<ProductDto>>(result.Data),
            PageIndex = result.PageIndex,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        });
            
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await productRepository.GetProductWithDetailsAsync(id);
        if (product is null) return NotFound("Produit introuvable.");
        return Ok(mapper.Map<ProductDto>(product));
    }

    [HttpGet("shop/{shopId:int}")]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetProductsByShop(
        int shopId, [FromQuery] ProductParams p)
    {
        var shop = await shopRepository.GetByIdAsync(shopId);
        if (shop is null) return NotFound("Shop introuvable.");
        p.ShopId = shopId;
        var result = await productRepository.GetProductsAsync(p);
        return Ok(new PagedResult<ProductDto>
        {
            Data = mapper.Map<List<ProductDto>>(result.Data),
            PageIndex = result.PageIndex,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        });
            
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<ProductDto>>> Search(
        [FromQuery] string q, [FromQuery] ProductParams p)
    {
        if (string.IsNullOrEmpty(q))
            return BadRequest("Veuillez entrer un terme de recherche.");
        p.Search = q;
        var result = await productRepository.GetProductsAsync(p);
        return Ok(new PagedResult<ProductDto>
        {
            Data = mapper.Map<List<ProductDto>>(result.Data),
            PageIndex = result.PageIndex,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        });
    }

    // ───────────────────────────────────────────
    // PROMOTIONS SUR PRODUIT
    // ───────────────────────────────────────────

    [HttpGet("{id:int}/promotions")]
    public async Task<ActionResult> GetProductPromotions(int id)
    {
        var product = await productRepository.GetProductWithDetailsAsync(id);
        if (product is null) return NotFound("Produit introuvable.");

        var now = DateTime.UtcNow;

       
        var debug = product.Promotions.Select(p => new {
            p.Id,
            p.IsActive,
            p.StartDate,
            p.EndDate,
            now
        });

        var activePromotions = product.Promotions
            .Where(p => p.IsActive && p.StartDate <= now && p.EndDate >= now)
            .Select(p => mapper.Map<PromotionDto>(p))
            .ToList();

        return Ok(new
        {
            productId = product.Id,
            productName = product.Name,
            activePromotions,
            totalPromotions = product.Promotions.Count,
            debug 
        });
    }

    [HttpPost("{id:int}/promotions/{promotionId:int}")]
    [Authorize(Roles = "Admin,Owner")]
    public async Task<ActionResult> AddPromotion(int id, int promotionId)
    {
        var product = await productRepository.GetProductWithDetailsAsync(id);
        if (product is null) return NotFound("Produit introuvable.");

        var shop = await shopRepository.GetByIdAsync(product.ShopId);
        if (!CanAccessShop(shop!)) return Forbidden();

        var promotion = await promotionRepository.GetByIdAsync(promotionId);
        if (promotion is null) return NotFound("Promotion introuvable.");

        if (promotion.ShopId != product.ShopId)
            return BadRequest("La promotion n'appartient pas au même shop que le produit.");

        if (product.Promotions.Any(p => p.Id == promotionId))
            return BadRequest("Cette promotion est déjà appliquée sur ce produit.");

        product.Promotions.Add(promotion);
        await productRepository.SaveChangesAsync();

        return Ok(new { message = "Promotion appliquée avec succès." });
    }

    [HttpPost("{id:int}/promotions/create")]
    [Authorize(Roles = "Admin,Owner")]
    public async Task<ActionResult> CreateAndAddPromotion(int id, CreatePromotionDto dto)
    {
        var product = await productRepository.GetProductWithDetailsAsync(id);
        if (product is null) return NotFound("Produit introuvable.");

        var shop = await shopRepository.GetByIdAsync(product.ShopId);
        if (!CanAccessShop(shop!)) return Forbidden();

        if (dto.Type == PromoType.Percentage && dto.DiscountPercentage is null)
            return BadRequest("Le pourcentage de réduction est requis.");

        if (dto.Type == PromoType.ForXGetY && (dto.BuyQuantity is null || dto.GetQuantity is null))
            return BadRequest("BuyQuantity et GetQuantity sont requis.");

        if (dto.StartDate >= dto.EndDate)
            return BadRequest("La date de début doit être avant la date de fin.");

        var promo = mapper.Map<Promotion>(dto);
        promo.ShopId = product.ShopId;
        promo.Products.Add(product);

        await promotionRepository.AddAsync(promo);
        await promotionRepository.SaveChangesAsync();

        return Ok(new
        {
            message = "Promotion créée et appliquée au produit avec succès.",
            promotionId = promo.Id,
            productId = product.Id
        });
    }

    [HttpGet("{id:int}/price")]
    public async Task<ActionResult> GetPrice(int id)
    {
        var product = await productRepository.GetProductWithDetailsAsync(id);
        if (product is null) return NotFound("Produit introuvable.");

        var shop = await shopRepository.GetByIdAsync(product.ShopId);
        if (shop is null) return NotFound("Shop introuvable.");

        var (promo, finalPrice) = PromoResolver.Resolve(product, 1, shop.PromoStrategy);

        return Ok(new
        {
            productId = product.Id,
            productName = product.Name,
            originalPrice = product.SellPrice,
            finalPrice,
            unitOfPrice = product.UnitOfPrice,
            activePromotion = promo is null ? null : mapper.Map<PromotionDto>(promo)
        });
    }

    [HttpDelete("{id:int}/promotions/{promotionId:int}")]
    [Authorize(Roles = "Admin,Owner")]
    public async Task<ActionResult> RemovePromotion(int id, int promotionId)
    {
        var product = await productRepository.GetProductWithDetailsAsync(id);
        if (product is null) return NotFound("Produit introuvable.");

        var shop = await shopRepository.GetByIdAsync(product.ShopId);
        if (!CanAccessShop(shop!)) return Forbidden();

        var promotion = product.Promotions.FirstOrDefault(p => p.Id == promotionId);
        if (promotion is null) return NotFound("Promotion non trouvée sur ce produit.");

        product.Promotions.Remove(promotion);
        await productRepository.SaveChangesAsync();

        return Ok(new { message = "Promotion retirée avec succès." });
    }

    // ───────────────────────────────────────────
    // CRUD
    // ───────────────────────────────────────────

    [HttpPost("shop/{shopId:int}")]
    [Authorize(Roles = "Admin,Owner")]
    public async Task<ActionResult<ProductDto>> CreateProduct(int shopId, CreateProductDto dto)
    {
        var shop = await shopRepository.GetByIdAsync(shopId);
        if (shop is null) return NotFound("Shop introuvable.");
        if (!CanAccessShop(shop)) return Forbidden();

        var product = mapper.Map<Product>(dto);
        product.ShopId = shopId;

        await productRepository.AddAsync(product);
        await productRepository.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, mapper.Map<ProductDto>(product));
    }

[HttpPut("{id:int}")]
[Authorize(Roles = "Admin,Owner")]
public async Task<ActionResult> UpdateProduct(int id, UpdateProductDto dto)
{
    var product = await productRepository.GetByIdAsync(id);
    if (product is null) return NotFound("Produit introuvable.");

    var shop = await shopRepository.GetByIdAsync(product.ShopId);
    if (!CanAccessShop(shop!)) return Forbidden();

    
    bool priceChanged = (dto.SellPrice.HasValue && dto.SellPrice != product.SellPrice)
                     || (dto.BuyPrice.HasValue && dto.BuyPrice != product.BuyPrice);

    
    var oldSellPrice = product.SellPrice;
    var oldBuyPrice = product.BuyPrice;

    mapper.Map(dto, product);
    product.UpdatedAt = DateTime.UtcNow;

    
    if (priceChanged)
    {
        var priceHistory = new PriceHistory
        {
            ProductId = product.Id,
            SellPrice = oldSellPrice,
            BuyPrice = oldBuyPrice,
            UnitOfPrice = product.UnitOfPrice.ToString(),
            ChangeReason = "Mise à jour manuelle",
            ChangedAt = DateTime.UtcNow
        };
        product.PriceHistories.Add(priceHistory);
    }

    await productRepository.UpdateAsync(product);
    await productRepository.SaveChangesAsync();

    return Ok(new { message = "Produit mis à jour avec succès." });
}

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Owner")]
    public async Task<ActionResult> DeleteProduct(int id)
    {
        var product = await productRepository.GetByIdAsync(id);
        if (product is null) return NotFound("Produit introuvable.");

        var shop = await shopRepository.GetByIdAsync(product.ShopId);
        if (!CanAccessShop(shop!)) return Forbidden();

        await productRepository.DeleteAsync(product);
        await productRepository.SaveChangesAsync();

        return Ok(new { message = "Produit supprimé avec succès." });
    }

    // ───────────────────────────────────────────
    // COPIE
    // ───────────────────────────────────────────

    [HttpPost("{id:int}/copy-to/{destinationShopId:int}")]
    [Authorize(Roles = "Admin,Owner")]
    public async Task<ActionResult<ProductDto>> CopyProduct(int id, int destinationShopId)
    {
        var product = await productRepository.GetProductWithDetailsAsync(id);
        if (product is null) return NotFound("Produit introuvable.");

        var destinationShop = await shopRepository.GetByIdAsync(destinationShopId);
        if (destinationShop is null) return NotFound("Shop de destination introuvable.");
        if (!CanAccessShop(destinationShop)) return Forbidden();

        var copy = mapper.Map<Product>(product);
        copy.ShopId = destinationShopId;

        await productRepository.AddAsync(copy);
        await productRepository.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProduct), new { id = copy.Id }, mapper.Map<ProductDto>(copy));
    }

    // ───────────────────────────────────────────
    // HISTORIQUE DES PRIX
    // ───────────────────────────────────────────

    [HttpGet("{id:int}/price-history")]
    public async Task<ActionResult> GetPriceHistory(int id)
    {
        var product = await productRepository.GetProductWithDetailsAsync(id);
        if (product is null) return NotFound("Produit introuvable.");

        return Ok(new
        {
            productId = product.Id,
            productName = product.Name,
            currentPrice = product.SellPrice,
            history = product.PriceHistories
                .OrderByDescending(ph => ph.ChangedAt)
                .Select(ph => mapper.Map<PriceHistoryDto>(ph))
                .ToList()
        });
    }

    // ───────────────────────────────────────────
    // HELPER
    // ───────────────────────────────────────────

    private bool CanAccessShop(Shop shop)
    {
        if (User.IsInRole("Admin")) return true;
        return shop.OwnerId == GetCurrentUserId();
    }
}