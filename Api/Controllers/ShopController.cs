using Core.DTOs.Shop;
using Core.Entities;
using Core.Helpers;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace Api.Controllers;

[Route("shops")]
public class ShopController(
    IShopRepository shopRepository, 
    IMapper mapper, 
    SignInManager<AppUser> signInManager,
    UserManager<AppUser> userManager) : BaseApiController
{
    private string? GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);
    private string? GetEffectiveOwnerId(string? ownerId) =>
        User.IsInRole("Admin") ? ownerId : GetCurrentUserId();

    // ───────────────────────────────────────────
    // LECTURE
    // ───────────────────────────────────────────

    [HttpGet]
    public async Task<ActionResult<PagedResult<ShopDto>>> GetAllShops([FromQuery] ShopParams p)
        => Ok(await shopRepository.GetShopsAsync(p));

    [HttpGet("open-now")]
    public async Task<ActionResult<PagedResult<ShopDto>>> GetOpenShops([FromQuery] ShopParams p)
        => Ok(await shopRepository.GetOpenShopsNowAsync(p));

    [HttpGet("{id:int}/store")]
    public async Task<ActionResult<ShopDto>> GetShopDetails(int id)
    {
        var shop = await shopRepository.GetShopWithDetailsAsync(id);
        return shop is null ? NotFound("Shop introuvable.") : Ok(shop);
    }

    [HttpGet("my-shops")]
    [Authorize(Roles = "Owner")]
    public async Task<ActionResult<PagedResult<ShopDto>>> GetMyShops([FromQuery] PaginationParams p)
    {
        var ownerId = GetCurrentUserId();
        if (string.IsNullOrEmpty(ownerId))
            return Unauthorized("Utilisateur non identifié.");

        return Ok(await shopRepository.GetByOwnerIdAsync(ownerId, p));
    }

    [HttpGet("by-owner/{ownerId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<ShopDto>>> GetShopsByOwner(string ownerId, [FromQuery] PaginationParams p)
        => Ok(await shopRepository.GetByOwnerIdAsync(ownerId, p));

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Owner")]
    public async Task<ActionResult<ShopDto>> GetShop(int id, [FromQuery] string? ownerId)
    {
        var shop = await shopRepository.GetShopWithDetailsAsync(id);
        if (shop is null) return NotFound("Shop introuvable.");

        var effectiveOwnerId = GetEffectiveOwnerId(ownerId);
        if (string.IsNullOrEmpty(effectiveOwnerId))
            return Unauthorized("Utilisateur non identifié.");
        if (shop.OwnerId != effectiveOwnerId) return Forbidden();

        return Ok(shop);
    }

    // ───────────────────────────────────────────
    // CRUD
    // ───────────────────────────────────────────

    [HttpPost]
    [Authorize(Roles = "Owner")]
    public async Task<ActionResult<ShopDto>> CreateShop(CreateShopDto dto)
    {
        var ownerId = GetCurrentUserId();
        if (string.IsNullOrEmpty(ownerId))
            return Unauthorized("Utilisateur non identifié.");

        var shop = mapper.Map<Shop>(dto);
        shop.OwnerId = ownerId;

        await shopRepository.AddAsync(shop);
        await shopRepository.SaveChangesAsync();

        return CreatedAtAction(nameof(GetShop), new { id = shop.Id }, mapper.Map<ShopDto>(shop));
    }

    [HttpPost("admin/create-for-owner")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ShopDto>> CreateShopForOwner(CreateShopForOwnerDto dto)
    {
        var owner = await userManager.FindByIdAsync(dto.OwnerId);
        if (owner == null)
            return NotFound("User not found.");

        var roles = await userManager.GetRolesAsync(owner);
        if (!roles.Contains("Owner"))
            return BadRequest("The specified user does not have the Owner role.");

        var shop = mapper.Map<Shop>(dto);
        await shopRepository.AddAsync(shop);
        await shopRepository.SaveChangesAsync();

        return CreatedAtAction(nameof(GetShop), new { id = shop.Id }, mapper.Map<ShopDto>(shop));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Owner")]
    public async Task<ActionResult> UpdateShop(int id, UpdateShopDto dto)
    {
        var shop = await shopRepository.GetByIdAsync(id);
        if (shop is null) return NotFound("Shop introuvable.");

        var effectiveOwnerId = GetEffectiveOwnerId(dto.OwnerId);
        if (string.IsNullOrEmpty(effectiveOwnerId))
            return Unauthorized("Utilisateur non identifié.");
        if (shop.OwnerId != effectiveOwnerId) return Forbidden();

        mapper.Map(dto, shop);
        shop.UpdatedAt = DateTime.UtcNow;

        await shopRepository.UpdateAsync(shop);
        await shopRepository.SaveChangesAsync();

        return Ok(new { message = "Shop mis à jour avec succès." });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Owner")]
    public async Task<ActionResult> DeleteShop(int id, [FromQuery] string? ownerId)
    {
        var shop = await shopRepository.GetByIdAsync(id);
        if (shop is null) return NotFound("Shop introuvable.");

        var effectiveOwnerId = GetEffectiveOwnerId(ownerId);
        if (string.IsNullOrEmpty(effectiveOwnerId))
            return Unauthorized("Utilisateur non identifié.");
        if (shop.OwnerId != effectiveOwnerId) return Forbidden();

        await shopRepository.DeleteAsync(shop);
        await shopRepository.SaveChangesAsync();

        return Ok(new { message = "Shop supprimé avec succès." });
    }
}