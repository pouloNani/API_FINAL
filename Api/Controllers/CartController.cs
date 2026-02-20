using Core.DTOs.Cart;
using Core.DTOs.Guest;
using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;

[Route("carts")]
public class CartController(ICartService cartService) : BaseApiController
{
    private string GetCurrentUserId()
    {
        // User connecté → JWT
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is not null) return userId;

        // Guest existant → cookie
        if (Request.Cookies.TryGetValue("guestId", out var guestId)
            && !string.IsNullOrEmpty(guestId))
            return $"guest:{guestId}";

        // Nouveau guest → générer GUID et cookie
        var newGuestId = Guid.NewGuid().ToString();
        Response.Cookies.Append("guestId", newGuestId, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });

        return $"guest:{newGuestId}";
    }

    // ───────────────────────────────────────────
    // LECTURE
    // ───────────────────────────────────────────

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CartSummaryDto>>> GetMyCarts()
    {
        var carts = await cartService.GetAllCartsAsync(GetCurrentUserId());
        return Ok(carts.Select(c => new CartSummaryDto
        {
            Id = c.Id,
            Name = c.Name,
            TotalAmount = c.TotalAmount,
            TotalDiscount = c.TotalDiscount,
            ItemCount = c.Items.Count,
            UpdatedAt = c.UpdatedAt
        }));
    }

    [HttpGet("{cartId}")]
    public async Task<ActionResult<CartDto>> GetCart(string cartId)
    {
        var cart = await cartService.GetCartAsync(GetCurrentUserId(), cartId);
        if (cart is null) return NotFound("Cart introuvable.");
        return Ok(MapToDto(cart));
    }

    // ───────────────────────────────────────────
    // CRUD CARTS
    // ───────────────────────────────────────────

    [HttpPost]
    public async Task<ActionResult<CartDto>> CreateCart(CreateCartDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Le nom du cart est requis.");

        try
        {
            var cart = await cartService.CreateCartAsync(GetCurrentUserId(), dto.Name);
            return CreatedAtAction(nameof(GetCart), new { cartId = cart.Id }, MapToDto(cart));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("{cartId}/rename")]
    public async Task<ActionResult<CartDto>> RenameCart(string cartId, RenameCartDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Le nom est requis.");

        var cart = await cartService.RenameCartAsync(GetCurrentUserId(), cartId, dto.Name);
        if (cart is null) return NotFound("Cart introuvable.");
        return Ok(MapToDto(cart));
    }

    [HttpDelete("{cartId}")]
    public async Task<ActionResult> DeleteCart(string cartId)
    {
        var deleted = await cartService.DeleteCartAsync(GetCurrentUserId(), cartId);
        if (!deleted) return NotFound("Cart introuvable.");
        return Ok(new { message = "Cart supprimé avec succès." });
    }

    // ───────────────────────────────────────────
    // CRUD ITEMS
    // ───────────────────────────────────────────

    [HttpPost("{cartId}/items")]
    public async Task<ActionResult<CartDto>> AddItem(string cartId, AddCartItemDto dto)
    {
        if (dto.Quantity <= 0)
            return BadRequest("La quantité doit être supérieure à 0.");

        if (cartId == "new")
        {
            try
            {
                var newCart = await cartService.CreateCartAsync(
                    GetCurrentUserId(), $"Mon panier {DateTime.UtcNow:dd/MM/yyyy}");
                cartId = newCart.Id;
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        var cart = await cartService.AddItemAsync(GetCurrentUserId(), cartId, dto);
        if (cart is null) return NotFound("Cart ou produit introuvable.");
        return Ok(MapToDto(cart));
    }

    [HttpPut("{cartId}/items/{productId:int}")]
    public async Task<ActionResult<CartDto>> UpdateItem(
        string cartId, int productId, UpdateCartItemDto dto)
    {
        if (dto.Quantity < 0)
            return BadRequest("La quantité ne peut pas être négative.");

        var cart = await cartService.UpdateItemQuantityAsync(
            GetCurrentUserId(), cartId, productId, dto.Quantity);

        if (cart is null) return NotFound("Cart ou produit introuvable.");
        return Ok(MapToDto(cart));
    }

    [HttpDelete("{cartId}/items/{productId:int}")]
    public async Task<ActionResult<CartDto>> RemoveItem(string cartId, int productId)
    {
        var cart = await cartService.RemoveItemAsync(GetCurrentUserId(), cartId, productId);
        if (cart is null) return NotFound("Cart ou produit introuvable.");
        return Ok(MapToDto(cart));
    }

    // ───────────────────────────────────────────
    // CHECKOUT
    // ───────────────────────────────────────────

    [HttpPost("{cartId}/checkout")]
    public async Task<ActionResult> Checkout(string cartId, [FromBody] GuestCheckoutDto? guestInfo = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isGuest = userId is null;

        if (isGuest)
        {
            if (string.IsNullOrEmpty(guestInfo?.GuestEmail))
                return BadRequest("Email requis.");
            if (string.IsNullOrEmpty(guestInfo?.GuestFirstName))
                return BadRequest("Prénom requis.");
            if (string.IsNullOrEmpty(guestInfo?.GuestLastName))
                return BadRequest("Nom requis.");
            if (string.IsNullOrEmpty(guestInfo?.GuestStreet))
                return BadRequest("Rue requise.");
            if (string.IsNullOrEmpty(guestInfo?.GuestCity))
                return BadRequest("Ville requise.");
            if (string.IsNullOrEmpty(guestInfo?.GuestZipCode))
                return BadRequest("Code postal requis.");
            if (string.IsNullOrEmpty(guestInfo?.GuestCountry))
                return BadRequest("Pays requis.");
        }

        try
        {
            var billId = await cartService.CheckoutAsync(
                GetCurrentUserId(), cartId, isGuest ? guestInfo : null);

            

            return Ok(new { message = "Commande validée avec succès.", billId });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // ───────────────────────────────────────────
    // HELPER
    // ───────────────────────────────────────────

    private static CartDto MapToDto(CartModel cart) => new()
    {
        Id = cart.Id,
        Name = cart.Name,
        TotalAmount = cart.TotalAmount,
        TotalDiscount = cart.TotalDiscount,
        TotalBeforeDiscount = cart.TotalBeforeDiscount,
        UpdatedAt = cart.UpdatedAt,
        CreatedAt = cart.CreatedAt,
        Items = cart.Items.Select(i => new CartItemDto
        {
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            ShopId = i.ShopId,
            ShopName = i.ShopName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            FinalPrice = i.FinalPrice,
            Discount = i.Discount,
            LineTotal = i.LineTotal,
            AppliedPromoName = i.AppliedPromoName,
            AppliedPromoType = i.AppliedPromoType,
            AppliedDiscountPercentage = i.AppliedDiscountPercentage,
            AppliedBuyQuantity = i.AppliedBuyQuantity,
            AppliedGetQuantity = i.AppliedGetQuantity
        }).ToList()
    };
}