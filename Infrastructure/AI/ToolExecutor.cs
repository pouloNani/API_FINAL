using System.Text.Json;
using Core.DTOs.Agent;
using Core.Entities;
using Core.Helpers;
using Core.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
namespace Infrastructure.AI;



public class ToolExecutor
{
    private readonly IShopRepository    _shopRepo;
    private readonly IProductRepository _productRepo;
    private readonly ICartService       _cartService;
    private readonly UserManager<AppUser>   _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ITokenService          _tokenService;

    public ToolExecutor(
    IShopRepository        shopRepo,
    IProductRepository     productRepo,
    ICartService           cartService,
    UserManager<AppUser>   userManager,
    SignInManager<AppUser> signInManager,
    ITokenService          tokenService)
{
    _shopRepo      = shopRepo;
    _productRepo   = productRepo;
    _cartService   = cartService;
    _userManager   = userManager;
    _signInManager = signInManager;
    _tokenService  = tokenService;
}
   

    // Capture les ProposedActions extraites pendant l'exécution
    public List<ProposedAction> CapturedActions { get; private set; } = new();

    public void ResetActions() => CapturedActions = new();

    public async Task<string> ExecuteAsync(string toolName, JsonElement args)
    {
        try
        {
            return toolName switch
            {
                "search_shops"            => await SearchShops(args),
                "get_open_shops"          => await GetOpenShops(args),
                "search_products"         => await SearchProducts(args),
                "get_products_by_shop"    => await GetProductsByShop(args),
                "get_product_price"       => await GetProductPrice(args),
                "get_promotions_by_shop"  => await GetPromotionsByShop(args),
                "search_best_promotions"  => await SearchBestPromotions(args),
                "get_cart"               => await GetCart(args),
                "propose_actions"        => ProposeActions(args),
                "register_user" => await RegisterUser(args),
                "login_user"    => await LoginUser(args),
                "get_profile"   => await GetProfile(args),
                "logout_user"   => LogoutUser(),

                _ => Err($"Tool '{toolName}' inconnu.")
            };
        }
        catch (Exception ex)
        {
            return Err($"Erreur lors de l'exécution de '{toolName}': {ex.Message}");
        }
    }

    // ─── SHOPS ───────────────────────────────────────────────────────

    private async Task<string> SearchShops(JsonElement args)
    {
        var p = new ShopParams
        {
            Name     = args.TryGet("name"),
            Type     = args.TryGet("type"),
            Category = args.TryGet("category"),
            PageSize = 8
        };

        var result = await _shopRepo.GetShopsAsync(p);

        if (!result.Data.Any())
            return Ok(new { message = "Aucun shop trouvé.", shops = Array.Empty<object>() });

        return Ok(new
        {
            totalFound = result.TotalCount,
            shops = result.Data.Select(s => new
            {
                s.Id,
                s.Name,
                s.Type,
                s.Category,
                s.Status,
                Address = s.Address == null ? null : new
                {
                    s.Address.FirstLine,
                    s.Address.SecondLine,
                    s.Address.City,
                    s.Address.PostalCode,
                    s.Address.State,
                    s.Address.Country
                }
            })
        });
    }

    private async Task<string> GetOpenShops(JsonElement args)
    {
        var p = new ShopParams
        {
            Category = args.TryGet("category"),
            PageSize = 8
        };

        var result = await _shopRepo.GetOpenShopsNowAsync(p);

        if (!result.Data.Any())
            return Ok(new { message = "Aucun shop ouvert en ce moment.", shops = Array.Empty<object>() });

        return Ok(new
        {
            totalFound = result.TotalCount,
            shops = result.Data.Select(s => new
            {
                s.Id,
                s.Name,
                s.Status,
                IsOpen = s.Status == ShopStatus.Open,
                Address = s.Address == null ? null : new
                {
                    s.Address.FirstLine,
                    s.Address.SecondLine,
                    s.Address.City,
                    s.Address.PostalCode,
                    s.Address.State,
                    s.Address.Country
                }
            })
        });
    }

    // ─── PRODUITS ────────────────────────────────────────────────────

private async Task<string> SearchProducts(JsonElement args)
        {
            var query = args.TryGet("query") ?? "";
            var words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                            .Select(w => w.ToLower())
                            .ToList();

            if (args.TryGetInt("shop_id", out var shopId)) {}
            if (args.TryGetDecimal("min_price", out var min)) {}
            if (args.TryGetDecimal("max_price", out var max)) {}

            PagedResult<Product> result;

            if (words.Count == 0)
            {
                var p = new ProductParams { PageSize = 8 };
                if (shopId > 0) p.ShopId = shopId;
                result = await _productRepo.GetProductsAsync(p);
            }
            else
            {
                // Cherche tous les produits qui matchent AU MOINS un mot
                var firstWord = words[0];
                var p = new ProductParams { Search = firstWord, PageSize = 50 };
                if (shopId > 0) p.ShopId = shopId;
                if (min > 0) p.MinPrice = min;
                if (max > 0) p.MaxPrice = max;

                result = await _productRepo.GetProductsAsync(p);

                // Score : nombre de mots matchants dans le nom
                var scored = result.Data
                    .Select(product => new
                    {
                        Product = product,
                        Score   = words.Count(w => product.Name.ToLower().Contains(w) ||
                                                product.Description.ToLower().Contains(w))
                    })
                    .Where(x => x.Score > 0)
                    .OrderByDescending(x => x.Score)
                    .Take(8)
                    .ToList();

                var sorted = scored.Select(x => x.Product).ToList();
                result = PagedResult<Product>.Create(sorted, sorted.Count, p);
            }

            if (!result.Data.Any())
                return Ok(new { message = $"Aucun produit trouvé pour '{query}'.", products = Array.Empty<object>() });

            return Ok(new
            {
                totalFound = result.TotalCount,
                products   = result.Data.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.SellPrice,
                    p.UnitOfPrice,
                    p.ShopId,
                    ShopName   = p.Shop?.Name,
                    ShopStatus = p.Shop?.Status.ToString(),
                    ShopIsOpen = p.Shop?.Status == ShopStatus.Open,
                    HasPromo   = p.Promotions?.Any(pr =>
                        pr.IsActive &&
                        pr.StartDate <= DateTime.UtcNow &&
                        pr.EndDate   >= DateTime.UtcNow) ?? false
                })
            });
        }
    private async Task<string> GetProductsByShop(JsonElement args)
    {
        if (!args.TryGetInt("shop_id", out var shopId))
            return Err("shop_id est requis.");

        var p = new ProductParams
        {
            ShopId   = shopId,
            Search   = args.TryGet("category"),
            PageSize = 15
        };

        var result = await _productRepo.GetProductsAsync(p);

        return Ok(new
        {
            shopId,
            totalFound = result.TotalCount,
            products   = result.Data.Select(p => new
            {
                p.Id,
                p.Name,
                p.SellPrice,
                p.UnitOfPrice,
                p.Description
            })
        });
    }

    private async Task<string> GetProductPrice(JsonElement args)
    {
        if (!args.TryGetInt("product_id", out var productId))
            return Err("product_id est requis.");

        var product = await _productRepo.GetProductWithDetailsAsync(productId);
        if (product is null) return Err("Produit introuvable.");

        var shop = await _shopRepo.GetByIdAsync(product.ShopId);
        if (shop is null) return Err("Shop introuvable.");

        var (promo, finalPrice) = PromoResolver.Resolve(product, 1, shop.PromoStrategy);

        return Ok(new
        {
            productId     = product.Id,
            productName   = product.Name,
            originalPrice = product.SellPrice,
            finalPrice,
            hasPromo      = promo is not null,
            promoName     = promo?.Name,
            promoType     = promo?.Type.ToString(),
            discount      = promo?.DiscountPercentage
        });
    }

    // ─── PROMOTIONS ──────────────────────────────────────────────────

    private async Task<string> GetPromotionsByShop(JsonElement args)
    {
        if (!args.TryGetInt("shop_id", out var shopId))
            return Err("shop_id est requis.");

        var products = await _productRepo.GetProductsWithActivePromotionsAsync(shopId);

        if (!products.Any())
            return Ok(new { message = "Aucune promotion active dans ce shop.", promos = Array.Empty<object>() });

        var now = DateTime.UtcNow;

        var promoItems = products
            .SelectMany(p => p.Promotions
                .Where(pr => pr.IsActive && pr.StartDate <= now && pr.EndDate >= now)
                .Select(pr => new
                {
                    productId   = p.Id,
                    productName = p.Name,
                    shopId      = p.ShopId,
                    originalPrice = p.SellPrice,
                    promoId     = pr.Id,
                    promoName   = pr.Name,
                    promoType   = pr.Type.ToString(),
                    discount    = pr.DiscountPercentage,
                    buyQty      = pr.BuyQuantity,
                    getQty      = pr.GetQuantity,
                    endsAt      = pr.EndDate
                }))
            .OrderByDescending(x => x.discount)
            .ToList();

        return Ok(new { totalPromos = promoItems.Count, promos = promoItems });
    }

    private async Task<string> SearchBestPromotions(JsonElement args)
    {
        var query = args.TryGet("query");

        // Récupérer tous les shops
        var allShops = await _shopRepo.GetShopsAsync(new ShopParams { PageSize = 100 });
        var now      = DateTime.UtcNow;
        var allPromos = new List<object>();

        foreach (var shop in allShops.Data)
        {
            var products = await _productRepo.GetProductsWithActivePromotionsAsync(shop.Id);

            foreach (var product in products)
            {
                // Filtrer par nom si query fourni
                if (!string.IsNullOrEmpty(query) &&
                    !product.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                    continue;

                foreach (var pr in product.Promotions
                    .Where(pr => pr.IsActive && pr.StartDate <= now && pr.EndDate >= now))
                {
                    var (_, finalPrice) = PromoResolver.Resolve(product, 1, shop.PromoStrategy);
                    var savings = product.SellPrice - finalPrice;

                    allPromos.Add(new
                    {
                        productId     = product.Id,
                        productName   = product.Name,
                        shopId        = shop.Id,
                        shopName      = shop.Name,
                        shopStatus    = shop.Status.ToString(),
                        shopIsOpen    = shop.Status == ShopStatus.Open,
                        originalPrice = product.SellPrice,
                        finalPrice,
                        savings,
                        promoName     = pr.Name,
                        promoType     = pr.Type.ToString(),
                        discount      = pr.DiscountPercentage,
                        buyQty        = pr.BuyQuantity,
                        getQty        = pr.GetQuantity,
                        endsAt        = pr.EndDate
                    });
                }
            }
        }

        if (!allPromos.Any())
            return Ok(new { message = "Aucune promotion active en ce moment.", promos = Array.Empty<object>() });

        // Trier par économie décroissante
        var sorted = allPromos
            .OrderByDescending(x => ((dynamic)x).savings)
            .Take(10)
            .ToList();

        return Ok(new { totalFound = sorted.Count, bestPromos = sorted });
    }

    // ─── PANIER ──────────────────────────────────────────────────────

    private async Task<string> GetCart(JsonElement args)
    {
        var userId = args.TryGet("user_id");
        if (string.IsNullOrEmpty(userId)) return Err("user_id est requis.");

        var carts = await _cartService.GetAllCartsAsync(userId);

        if (!carts.Any())
            return Ok(new { message = "Aucun panier trouvé.", carts = Array.Empty<object>() });

        return Ok(new
        {
            totalCarts = carts.Count,
            carts = carts.Select(c => new
            {
                c.Id,
                c.Name,
                c.TotalAmount,
                c.TotalDiscount,
                itemCount = c.Items.Count,
                items = c.Items.Select(i => new
                {
                    i.ProductId,
                    i.ProductName,
                    i.ShopName,
                    i.Quantity,
                    i.FinalPrice,
                    i.LineTotal,
                    i.AppliedPromoName
                })
            })
        });
    }

    // ─── PROPOSE ACTIONS ─────────────────────────────────────────────

    private string ProposeActions(JsonElement args)
    {
        // Extraire et capturer les actions proposées par l'IA
        if (args.TryGetProperty("actions", out var actionsEl))
        {
            foreach (var action in actionsEl.EnumerateArray())
            {
                CapturedActions.Add(new ProposedAction
                {
                    Label       = action.TryGet("label")       ?? "",
                    Description = action.TryGet("description") ?? "",
                    ActionType  = action.TryGet("action_type") ?? "",
                    ProductId   = action.TryGetInt("product_id", out var pid) ? pid : 0,
                    ShopId      = action.TryGetInt("shop_id",    out var sid) ? sid : 0,
                    Quantity    = action.TryGetInt("quantity",   out var qty) ? qty : 1
                });
            }
        }

        return Ok(new { status = "proposed", message = "Actions présentées au client, en attente de sa confirmation." });
    }

    private async Task<string> RegisterUser(JsonElement args)
{
    var email    = args.TryGet("email");
    var password = args.TryGet("password");

    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        return Err("Email et mot de passe requis.");

    var user = new AppUser { Email = email, UserName = email };
    var result = await _userManager.CreateAsync(user, password);

    if (!result.Succeeded)
        return Err(string.Join(", ", result.Errors.Select(e => e.Description)));

    await _userManager.AddToRoleAsync(user, "Client");
    var token = _tokenService.CreateToken(user);

    return Ok(new
    {
        success   = true,
        message   = "Compte créé avec succès !",
        userId    = user.Id,
        email     = user.Email,
        token,                    // JWT retourné à stocker côté client
        role      = "Client"
    });
}

private async Task<string> LoginUser(JsonElement args)
{
    var email    = args.TryGet("email");
    var password = args.TryGet("password");

    var user = await _userManager.FindByEmailAsync(email!);
    if (user is null) return Err("Email ou mot de passe incorrect.");

    var result = await _signInManager
        .CheckPasswordSignInAsync(user, password!, false);

    if (!result.Succeeded) return Err("Email ou mot de passe incorrect.");

    var token = _tokenService.CreateToken(user);
    var roles = await _userManager.GetRolesAsync(user);

    return Ok(new
    {
        success = true,
        message = $"Bienvenue {user.Email} !",
        userId  = user.Id,
        email   = user.Email,
        token,
        role    = roles.FirstOrDefault()
    });
}

    private async Task<string> GetProfile(JsonElement args)
    {
        var userId = args.TryGet("user_id");
        var user   = await _userManager.FindByIdAsync(userId!);
        if (user is null) return Err("Utilisateur introuvable.");

        return Ok(new { user.Id, user.Email, user.UserName });
    }

    private string LogoutUser() =>
        Ok(new { message = "Déconnecté avec succès." });

    // ─── HELPERS ─────────────────────────────────────────────────────

    private static string Ok(object data) =>
        JsonSerializer.Serialize(new { success = true, data });

    private static string Err(string msg) =>
        JsonSerializer.Serialize(new { success = false, error = msg });
}