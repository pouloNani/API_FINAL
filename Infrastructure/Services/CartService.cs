using System;

namespace Infrastructure.Services;

using System.Text.Json;
using Core.DTOs.Cart;
using Core.DTOs.Guest;
using Core.Entities;
using Core.Helpers;
using Core.Interfaces;
using Core.Models;
using StackExchange.Redis;



public class CartService(
    IConnectionMultiplexer redis,
    IProductRepository productRepository,
    IShopRepository shopRepository,
    IBillRepository billRepository) : ICartService
{
    private readonly IDatabase _db = redis.GetDatabase();
    private static readonly TimeSpan Ttl = TimeSpan.FromDays(30);

    // ─── Clés Redis ──────────────────────────────────────────
    private static string CartKey(string userId, string cartId) =>
        $"cart:{userId}:{cartId}";

    private static string CartListKey(string userId) =>
        $"cart:{userId}:list";

    // ─── Gestion des carts ───────────────────────────────────

    public async Task<CartModel?> GetCartAsync(string userId, string cartId)
    {
        var data = await _db.StringGetAsync(CartKey(userId, cartId));
        if (data.IsNullOrEmpty) return null;

        var cart = JsonSerializer.Deserialize<CartModel>((string)data!);
        return cart;
    }

    public async Task<IReadOnlyList<CartModel>> GetAllCartsAsync(string userId)
    {
        var cartIds = await _db.SetMembersAsync(CartListKey(userId));
        var carts = new List<CartModel>();

        foreach (var cartId in cartIds)
        {
            var cart = await GetCartAsync(userId, cartId!);
            if (cart is not null)
                carts.Add(cart);
            else
                // Cart expiré — nettoyer la liste
                await _db.SetRemoveAsync(CartListKey(userId), cartId);
        }

        return carts.OrderByDescending(c => c.UpdatedAt).ToList();
    }

    public async Task<CartModel> CreateCartAsync(string userId, string name)
    {
         var existingCarts = await GetAllCartsAsync(userId);
                 if (existingCarts.Count >= 5)
                    throw new InvalidOperationException("Vous ne pouvez pas avoir plus de 5 carts simultanés.");
        var cart = new CartModel
        {
            Name = name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await SaveCartAsync(userId, cart);
        return cart;
    }

    public async Task<CartModel?> RenameCartAsync(string userId, string cartId, string name)
    {
        var cart = await GetCartAsync(userId, cartId);
        if (cart is null) return null;

        cart.Name = name;
        cart.UpdatedAt = DateTime.UtcNow;

        await SaveCartAsync(userId, cart);
        return cart;
    }

    public async Task<bool> DeleteCartAsync(string userId, string cartId)
    {
        var deleted = await _db.KeyDeleteAsync(CartKey(userId, cartId));
        await _db.SetRemoveAsync(CartListKey(userId), cartId);
        return deleted;
    }

    // ─── Gestion des items ───────────────────────────────────

    public async Task<CartModel?> AddItemAsync(string userId, string cartId, AddCartItemDto dto)
    {
        var cart = await GetCartAsync(userId, cartId);
        if (cart is null) return null;

        if (dto.Quantity <= 0) return null;

        var product = await productRepository.GetProductWithDetailsAsync(dto.ProductId);
        if (product is null) return null;

        var shop = await shopRepository.GetByIdAsync(product.ShopId);
        if (shop is null) return null;

        // Résolution de la promo
        var (resolvedPromo, finalPrice) = PromoResolver.Resolve(
            product, dto.Quantity, shop.PromoStrategy);

        // Si le produit existe déjà dans le cart → incrémenter
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
        if (existingItem is not null)
        {
            existingItem.Quantity += dto.Quantity;
            var (_, updatedFinalPrice) = PromoResolver.Resolve(
                product, existingItem.Quantity, shop.PromoStrategy);
            existingItem.FinalPrice = updatedFinalPrice;
            ApplyPromoSnapshot(existingItem, resolvedPromo);
        }
        else
        {
            cart.Items.Add(new CartItemModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ShopId = shop.Id,
                ShopName = shop.Name,
                Quantity = dto.Quantity,
                UnitPrice = product.SellPrice,
                FinalPrice = finalPrice,
                PromotionId = resolvedPromo?.Id,
                AppliedPromoName = resolvedPromo?.Name,
                AppliedPromoType = resolvedPromo?.Type.ToString(),
                AppliedDiscountPercentage = resolvedPromo?.DiscountPercentage,
                AppliedBuyQuantity = resolvedPromo?.BuyQuantity,
                AppliedGetQuantity = resolvedPromo?.GetQuantity
            });
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await SaveCartAsync(userId, cart);
        return cart;
    }

    public async Task<CartModel?> UpdateItemQuantityAsync(
        string userId, string cartId, int productId, int quantity)
    {
        var cart = await GetCartAsync(userId, cartId);
        if (cart is null) return null;

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null) return null;

        if (quantity <= 0)
        {
            // Quantité à 0 = retirer l'item
            cart.Items.Remove(item);
        }
        else
        {
            var product = await productRepository.GetProductWithDetailsAsync(productId);
            var shop = await shopRepository.GetByIdAsync(item.ShopId);

            if (product is null || shop is null) return null;

            var (resolvedPromo, finalPrice) = PromoResolver.Resolve(
                product, quantity, shop.PromoStrategy);

            item.Quantity = quantity;
            item.FinalPrice = finalPrice;
            ApplyPromoSnapshot(item, resolvedPromo);
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await SaveCartAsync(userId, cart);
        return cart;
    }

    public async Task<CartModel?> RemoveItemAsync(string userId, string cartId, int productId)
    {
        var cart = await GetCartAsync(userId, cartId);
        if (cart is null) return null;

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null) return null;

        cart.Items.Remove(item);
        cart.UpdatedAt = DateTime.UtcNow;

        await SaveCartAsync(userId, cart);
        return cart;
    }

    // ─── Checkout ────────────────────────────────────────────

    public async Task<int> CheckoutAsync(string userId, string cartId)
    {
        var cart = await GetCartAsync(userId, cartId);
        if (cart is null) throw new Exception("Cart introuvable.");
        if (!cart.Items.Any()) throw new Exception("Le cart est vide.");

        // Regrouper les items par shop
        var itemsByShop = cart.Items.GroupBy(i => i.ShopId);

        // Un cart peut avoir des produits de plusieurs shops
        // → on crée une Bill par shop
        int lastBillId = 0;

        foreach (var shopGroup in itemsByShop)
        {
            var shop = await shopRepository.GetByIdAsync(shopGroup.Key);
            if (shop is null) continue;

            var bill = new Bill
            {
                UserId = userId,
                ShopId = shopGroup.Key,
                BillNumber = GenerateBillNumber(),
                BilledAt = DateTime.UtcNow,
                Status = BillStatus.Pending
            };

            foreach (var item in shopGroup)
            {
                bill.BillItems.Add(new BillItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    FinalPrice = item.FinalPrice,
                    PromotionId = item.PromotionId
                });
            }

            await billRepository.AddAsync(bill);
            await billRepository.SaveChangesAsync();
            lastBillId = bill.Id;
        }

        // Supprimer le cart après checkout
        await DeleteCartAsync(userId, cartId);

        return lastBillId;
    }

    // ─── Helpers privés ──────────────────────────────────────

    private async Task SaveCartAsync(string userId, CartModel cart)
    {
        var key = CartKey(userId, cart.Id);
        var json = JsonSerializer.Serialize(cart);

        await _db.StringSetAsync(key, json, Ttl);
        await _db.SetAddAsync(CartListKey(userId), cart.Id);
        await _db.KeyExpireAsync(CartListKey(userId), Ttl);
    }

    private static void ApplyPromoSnapshot(CartItemModel item, Promotion? promo)
    {
        item.PromotionId = promo?.Id;
        item.AppliedPromoName = promo?.Name;
        item.AppliedPromoType = promo?.Type.ToString();
        item.AppliedDiscountPercentage = promo?.DiscountPercentage;
        item.AppliedBuyQuantity = promo?.BuyQuantity;
        item.AppliedGetQuantity = promo?.GetQuantity;
    }

    private static string GenerateBillNumber() =>
        $"BILL-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

    public async Task<int> CheckoutAsync(string userId, string cartId, GuestCheckoutDto? guestInfo = null)
    {
        var cart = await GetCartAsync(userId, cartId);
        if (cart is null) throw new Exception("Cart introuvable.");
        if (!cart.Items.Any()) throw new Exception("Le cart est vide.");

        var itemsByShop = cart.Items.GroupBy(i => i.ShopId);
        int lastBillId = 0;

        foreach (var shopGroup in itemsByShop)
        {
            var bill = new Bill
            {
                UserId = guestInfo is null ? userId : null,
                GuestId = guestInfo is not null ? userId : null,
                GuestEmail = guestInfo?.GuestEmail,
                GuestFirstName = guestInfo?.GuestFirstName,
                GuestLastName = guestInfo?.GuestLastName,
                ShopId = shopGroup.Key,
                BillNumber = GenerateBillNumber(),
                BilledAt = DateTime.UtcNow,
                Status = BillStatus.Pending
            };

            foreach (var item in shopGroup)
            {
                bill.BillItems.Add(new BillItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    FinalPrice = item.FinalPrice,
                    PromotionId = item.PromotionId
                });
            }

            await billRepository.AddAsync(bill);
            await billRepository.SaveChangesAsync();
            lastBillId = bill.Id;
        }

        await DeleteCartAsync(userId, cartId);
        return lastBillId;
    }
}