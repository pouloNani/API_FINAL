using System;

namespace Core.Interfaces;

using Core.Models;
using Core.DTOs.Cart;
using Core.DTOs.Guest;

public interface ICartService
{
    // ─── Gestion des carts ───────────────────────────────────
    Task<CartModel?> GetCartAsync(string userId, string cartId);
    Task<IReadOnlyList<CartModel>> GetAllCartsAsync(string userId);
    Task<CartModel> CreateCartAsync(string userId, string name);
    Task<CartModel?> RenameCartAsync(string userId, string cartId, string name);
    Task<bool> DeleteCartAsync(string userId, string cartId);

    // ─── Gestion des items ───────────────────────────────────
    Task<CartModel?> AddItemAsync(string userId, string cartId, AddCartItemDto dto);
    Task<CartModel?> UpdateItemQuantityAsync(string userId, string cartId, int productId, int quantity);
    Task<CartModel?> RemoveItemAsync(string userId, string cartId, int productId);

    // ─── Checkout ────────────────────────────────────────────
    Task<int> CheckoutAsync(string userId, string cartId, GuestCheckoutDto? guestInfo = null);


}