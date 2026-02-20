using AutoMapper;
using Core.DTOs.Promotion;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Core.DTOs.Product;

namespace Api.Controllers;

[Route("shops/{shopId:int}/promotions")]
public class PromotionController(
    IPromotionRepository promotionRepository,
    IShopRepository shopRepository,
    IProductRepository productRepository,
    IMapper mapper) : BaseApiController
{
    private string? GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private async Task<bool> CanAccessShop(int shopId)
    {
        var shop = await shopRepository.GetByIdAsync(shopId);
        if (shop is null) return false;
        if (User.IsInRole("Admin")) return true;
        return shop.OwnerId == GetCurrentUserId();
    }

    // ───────────────────────────────────────────
    // LECTURE
    // ───────────────────────────────────────────

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PromotionDto>>> GetPromotions(int shopId)
    {
        var shop = await shopRepository.GetByIdAsync(shopId);
        if (shop is null) return NotFound("Shop introuvable.");

        var promotions = await promotionRepository.GetByShopIdAsync(shopId);
        return Ok(mapper.Map<IReadOnlyList<PromotionDto>>(promotions));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PromotionDto>> GetPromotion(int shopId, int id)
    {
        var promotion = await promotionRepository.GetByIdAsync(id);
        if (promotion is null) return NotFound("Promotion introuvable.");
        if (promotion.ShopId != shopId) return NotFound("Promotion introuvable.");

        return Ok(mapper.Map<PromotionDto>(promotion));
    }

    [HttpGet("{id:int}/products")]
    public async Task<ActionResult> GetPromotionProducts(int shopId, int id)
    {
        var promotion = await promotionRepository.GetPromotionWithProductsAsync(id);
        if (promotion is null) return NotFound("Promotion introuvable.");
        if (promotion.ShopId != shopId) return NotFound("Promotion introuvable.");

        return Ok(new
        {
            promotionId = promotion.Id,
            promotionName = promotion.Name,
            products = mapper.Map<IReadOnlyList<ProductDto>>(promotion.Products)
        });
    }

    // ───────────────────────────────────────────
    // CRUD
    // ───────────────────────────────────────────

    [HttpPost]
    [Authorize(Roles = "Admin,Owner")]
    public async Task<ActionResult<PromotionDto>> CreatePromotion(int shopId, CreatePromotionDto dto)
    {
        if (!await CanAccessShop(shopId)) return Forbid();

        if (dto.Type == PromoType.Percentage && dto.DiscountPercentage is null)
            return BadRequest("Le pourcentage de réduction est requis.");

        if (dto.Type == PromoType.ForXGetY && (dto.BuyQuantity is null || dto.GetQuantity is null))
            return BadRequest("BuyQuantity et GetQuantity sont requis.");

        if (dto.StartDate >= dto.EndDate)
            return BadRequest("La date de début doit être avant la date de fin.");

        var promotion = mapper.Map<Promotion>(dto);
        promotion.ShopId = shopId;

        await promotionRepository.AddAsync(promotion);
        await promotionRepository.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPromotion),
            new { shopId, id = promotion.Id },
            mapper.Map<PromotionDto>(promotion));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Owner")]
    public async Task<ActionResult> UpdatePromotion(int shopId, int id, UpdatePromotionDto dto)
    {
        if (!await CanAccessShop(shopId)) return Forbid();

        var promotion = await promotionRepository.GetByIdAsync(id);
        if (promotion is null) return NotFound("Promotion introuvable.");
        if (promotion.ShopId != shopId) return NotFound("Promotion introuvable.");

        if (dto.StartDate.HasValue && dto.EndDate.HasValue && dto.StartDate >= dto.EndDate)
            return BadRequest("La date de début doit être avant la date de fin.");

        mapper.Map(dto, promotion);
        await promotionRepository.UpdateAsync(promotion);
        await promotionRepository.SaveChangesAsync();

        return Ok(new { message = "Promotion mise à jour avec succès." });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Owner")]
    public async Task<ActionResult> DeletePromotion(int shopId, int id)
    {
        if (!await CanAccessShop(shopId)) return Forbid();

        var promotion = await promotionRepository.GetByIdAsync(id);
        if (promotion is null) return NotFound("Promotion introuvable.");
        if (promotion.ShopId != shopId) return NotFound("Promotion introuvable.");

        await promotionRepository.DeleteAsync(promotion);
        await promotionRepository.SaveChangesAsync();

        return Ok(new { message = "Promotion supprimée avec succès." });
    }
    [HttpDelete("delete-all")]
    [Authorize(Roles = "Admin,Owner")]
    public async Task<ActionResult> DeleteAllPromotions(int shopId)
    {
        if (!await CanAccessShop(shopId)) return Forbid();

        var promotions = await promotionRepository.GetByShopIdAsync(shopId);
        if (!promotions.Any()) return NotFound("Aucune promotion trouvée.");

        foreach (var promo in promotions)
            await promotionRepository.DeleteAsync(promo);

        await promotionRepository.SaveChangesAsync();

        return Ok(new { message = $"{promotions.Count} promotion(s) supprimée(s)." });
    }

    // ───────────────────────────────────────────
    // GESTION DES PRODUITS
    // ───────────────────────────────────────────

    [HttpPost("{id:int}/products")]
    [Authorize(Roles = "Admin,Owner")]
    public async Task<ActionResult> AddProducts(int shopId, int id, [FromBody] List<int> productIds)
    {
        if (!await CanAccessShop(shopId)) return Forbid();

        var promotion = await promotionRepository.GetPromotionWithProductsAsync(id);
        if (promotion is null) return NotFound("Promotion introuvable.");
        if (promotion.ShopId != shopId) return NotFound("Promotion introuvable.");

        var added = new List<int>();
        var skipped = new List<int>();

        foreach (var productId in productIds)
        {
            var product = await productRepository.GetByIdAsync(productId);

            if (product is null || product.ShopId != shopId)
            {
                skipped.Add(productId);
                continue;
            }

            if (promotion.Products.Any(p => p.Id == productId))
            {
                skipped.Add(productId);
                continue;
            }

            promotion.Products.Add(product);
            added.Add(productId);
        }

        await promotionRepository.SaveChangesAsync();

        return Ok(new { message = "Produits ajoutés.", added, skipped });
    }

    [HttpDelete("{id:int}/products/{productId:int}")]
    [Authorize(Roles = "Admin,Owner")]
    public async Task<ActionResult> RemoveProduct(int shopId, int id, int productId)
    {
        if (!await CanAccessShop(shopId)) return Forbid();

        var promotion = await promotionRepository.GetPromotionWithProductsAsync(id);
        if (promotion is null) return NotFound("Promotion introuvable.");
        if (promotion.ShopId != shopId) return NotFound("Promotion introuvable.");

        var product = promotion.Products.FirstOrDefault(p => p.Id == productId);
        if (product is null) return NotFound("Produit non trouvé sur cette promotion.");

        promotion.Products.Remove(product);
        await promotionRepository.SaveChangesAsync();

        return Ok(new { message = "Produit retiré de la promotion." });
    }
}