using Core.DTOs.Agent;

namespace Infrastructure.AI;

public static class ToolDefinitions
{
    public static readonly List<ToolDefinition> All = new()
    
    {

                
        new()
        {
            Name        = "register_user",
            Description = "Inscrit un nouvel utilisateur. Appelle seulement quand tu as email ET mot de passe.",
            Parameters  = new
            {
                type       = "object",
                properties = new
                {
                    email     = new { type = "string" },
                    password  = new { type = "string" },
                    firstName = new { type = "string", description = "Optionnel" },
                    lastName  = new { type = "string", description = "Optionnel" }
                },
                required = new[] { "email", "password" }
            }
        },

        new()
        {
            Name        = "login_user",
            Description = "Connecte un utilisateur. Appelle seulement quand tu as email ET mot de passe.",
            Parameters  = new
            {
                type       = "object",
                properties = new
                {
                    email    = new { type = "string" },
                    password = new { type = "string" }
                },
                required = new[] { "email", "password" }
            }
        },

        new()
        {
            Name        = "get_profile",
            Description = "Récupère le profil de l'utilisateur connecté.",
            Parameters  = new
            {
                type       = "object",
                properties = new
                {
                    user_id = new { type = "string" }
                },
                required = new[] { "user_id" }
            }
        },

        new()
        {
            Name        = "logout_user",
            Description = "Déconnecte l'utilisateur.",
            Parameters  = new
            {
                type = "object", properties = new { }, required = Array.Empty<string>()
            }
        },
        // ─── SHOPS ───────────────────────────────────────────────

        new()
        {
            Name        = "search_shops",
            Description = "Cherche des shops par nom, ville, type ou catégorie. N'appelle ce tool QUE si tu as au moins un paramètre de recherche. Si le client veut voir tous les shops sans filtre → appelle search_shops sans aucun paramètre.",
            Parameters  = new
            {
                type       = "object",
                properties = new
                {
                    name     = new { type = "string", description = "Nom du shop" },
                    city     = new { type = "string", description = "Ville" },
                    type     = new { type = "string", description = "Type (ex: supermarché, boulangerie, pharmacie)" },
                    category = new { type = "string", description = "Catégorie (ex: alimentaire, électronique)" }
                },
                required = Array.Empty<string>()
            }
        },
        

        new()
        {
            Name        = "get_open_shops",
            Description = "Récupère uniquement les shops ouverts EN CE MOMENT. Utilise quand le client veut savoir ce qui est ouvert.",
            Parameters  = new
            {
                type       = "object",
                properties = new
                {
                    city     = new { type = "string", description = "Filtrer par ville (optionnel)" },
                    category = new { type = "string", description = "Filtrer par catégorie (optionnel)" }
                },
                required = Array.Empty<string>()
            }
        },

        // ─── PRODUITS ────────────────────────────────────────────

        new()
        {
            Name        = "search_products",
            Description = "Cherche des produits par nom dans tous les shops ou dans un shop précis. Utilise quand le client cherche un produit spécifique comme 'coca', 'pain', 'lait', 'iphone'.",
            Parameters  = new
            {
                type       = "object",
                properties = new
                {
                    query     = new { type = "string",  description = "Nom ou mot-clé du produit" },
                    shop_id   = new { type = "integer", description = "Filtrer par shop ID (optionnel)" },
                    min_price = new { type = "number",  description = "Prix minimum (optionnel)" },
                    max_price = new { type = "number",  description = "Prix maximum (optionnel)" }
                },
                required = Array.Empty<string>()

            }
        },

        new()
        {
            Name        = "get_products_by_shop",
            Description = "Liste les produits d'un shop. ATTENTION: shop_id doit être un entier obtenu via search_shops. Ne jamais inventer un shop_id. Si tu n'as pas de shop_id, appelle d'abord search_shops.",
            Parameters  = new
            {
                type       = "object",
                properties = new
                {
                    shop_id  = new { type = "integer", description = "ID du shop" },
                    category = new { type = "string",  description = "Filtrer par catégorie (optionnel)" }
                },
                required = new[] { "shop_id" }
            }
        },

        new()
        {
            Name        = "get_product_price",
            Description = "Obtient le prix final d'un produit avec promotions déjà appliquées.",
            Parameters  = new
            {
                type       = "object",
                properties = new
                {
                    product_id = new { type = "integer", description = "ID du produit" }
                },
                required = new[] { "product_id" }
            }
        },

        // ─── PROMOTIONS ──────────────────────────────────────────

        new()
        {
            Name        = "get_promotions_by_shop",
            Description = "Récupère tous les produits EN PROMOTION dans un shop donné. Utilise quand le client demande les promos d'un shop spécifique.",
            Parameters  = new
            {
                type       = "object",
                properties = new
                {
                    shop_id = new { type = "integer", description = "ID du shop" }
                },
                required = new[] { "shop_id" }
            }
        },

        new()
        {
            Name        = "search_best_promotions",
            Description = "Cherche les MEILLEURES promotions actives dans tous les shops ou par nom de produit. Utilise quand le client veut les meilleures offres, promos, réductions, ou bonnes affaires près de lui.",
            Parameters  = new
            {
                type       = "object",
                properties = new
                {
                    query = new { type = "string", description = "Nom du produit à chercher en promo (optionnel, laisser vide pour toutes les promos)" }
                },
                required = Array.Empty<string>()
            }
        },

        // ─── PANIER ──────────────────────────────────────────────

        new()
        {
            Name        = "get_cart",
            Description = "Affiche le contenu actuel du panier du client.",
            Parameters  = new
            {
                type       = "object",
                properties = new
                {
                    user_id = new { type = "string", description = "ID de l'utilisateur" }
                },
                required = new[] { "user_id" }
            }
        },

        // ─── PROPOSE ACTIONS (OBLIGATOIRE) ───────────────────────

        new()
        {
            Name        = "propose_actions",
            Description = @"OBLIGATOIRE après chaque recherche de produits ou shops.
Propose des actions cliquables au client SANS jamais les exécuter automatiquement.
Le client doit TOUJOURS confirmer avant qu'une action soit effectuée.
NE JAMAIS appeler add_to_cart directement — toujours passer par propose_actions d'abord.",
            Parameters  = new
            {
                type       = "object",
                properties = new
                {
                    message = new { type = "string", description = "Résumé clair des résultats pour le client" },
                    actions = new
                    {
                        type  = "array",
                        items = new
                        {
                            type       = "object",
                            properties = new
                            {
                                label       = new { type = "string",  description = "Texte du bouton ex: 'Ajouter au panier'" },
                                description = new { type = "string",  description = "Détail ex: 'Coca-Cola 33cl - Épicerie Hassan - 1.20€'" },
                                action_type = new { type = "string", @enum = new[] { "add_to_cart", "view_shop", "get_details", "navigate" } },
                                product_id  = new { type = "integer", description = "ID du produit (si add_to_cart)" },
                                shop_id     = new { type = "integer", description = "ID du shop" },
                                quantity    = new { type = "integer", description = "Quantité (défaut 1)" }
                            },
                            required = new[] { "label", "description", "action_type" }
                        }
                    }
                },
                required = new[] { "message", "actions" }
            }
        }
    };
}