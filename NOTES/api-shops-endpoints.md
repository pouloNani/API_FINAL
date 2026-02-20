# Données de test — Shops & Products

Base URL: `https://localhost:6001`
⚠️ Se connecter en admin avant toutes les requêtes POST/PUT/DELETE

---

## 1. CRÉER LES SHOPS

### Shop 1 — Boulangerie (Owner 1)
```
POST https://localhost:6001/shops/admin/create-for-owner
Content-Type: application/json

{
  "ownerId": "86f62e43-aa84-44d5-a195-20439d69aa85",
  "name": "Boulangerie Martin",
  "vatNumber": "BE0123456789",
  "type": "Physical",
  "status": "Open",
  "category": "Boulangerie",
  "promoStrategy": "BestForClient",
  "address": {
    "firstLine": "Rue de la Cathédrale 12",
    "city": "Liège",
    "state": "Liège",
    "postalCode": "4000",
    "country": "Belgium"
  }
}
```

### Shop 2 — Fromagerie (Owner 1)
```
POST https://localhost:6001/shops/admin/create-for-owner
Content-Type: application/json

{
  "ownerId": "86f62e43-aa84-44d5-a195-20439d69aa85",
  "name": "Fromagerie Dubois",
  "vatNumber": "BE0234567891",
  "type": "Physical",
  "status": "Open",
  "category": "Alimentaire",
  "promoStrategy": "BestForClient",
  "address": {
    "firstLine": "Rue Saint-Gilles 45",
    "city": "Liège",
    "state": "Liège",
    "postalCode": "4000",
    "country": "Belgium"
  }
}
```

### Shop 3 — Librairie (Owner 2)
```
POST https://localhost:6001/shops/admin/create-for-owner
Content-Type: application/json

{
  "ownerId": "dc433a93-f507-4d48-bbb5-684a4ffaff62",
  "name": "Librairie Lecomte",
  "vatNumber": "BE0345678912",
  "type": "Physical",
  "status": "Open",
  "category": "Multimedia",
  "promoStrategy": "BestForClient",
  "address": {
    "firstLine": "Boulevard de la Sauvenière 8",
    "secondLine": "Rez-de-chaussée",
    "city": "Liège",
    "state": "Liège",
    "postalCode": "4000",
    "country": "Belgium"
  }
}
```

### Shop 4 — Boucherie (Owner 2)
```
POST https://localhost:6001/shops/admin/create-for-owner
Content-Type: application/json

{
  "ownerId": "dc433a93-f507-4d48-bbb5-684a4ffaff62",
  "name": "Boucherie Renard",
  "vatNumber": "BE0456789123",
  "type": "Physical",
  "status": "Open",
  "category": "Boucherie",
  "promoStrategy": "BestForClient",
  "address": {
    "firstLine": "Rue Puits-en-Sock 22",
    "city": "Liège",
    "state": "Liège",
    "postalCode": "4000",
    "country": "Belgium"
  }
}
```

### Shop 5 — Multimédia (Owner 1)
```
POST https://localhost:6001/shops/admin/create-for-owner
Content-Type: application/json

{
  "ownerId": "86f62e43-aa84-44d5-a195-20439d69aa85",
  "name": "TechZone",
  "vatNumber": "BE0567891234",
  "type": "Physical",
  "status": "Open",
  "category": "Multimedia",
  "promoStrategy": "BestForClient",
  "address": {
    "firstLine": "Place Saint-Lambert 3",
    "city": "Liège",
    "state": "Liège",
    "postalCode": "4000",
    "country": "Belgium"
  }
}
```

### Shop 6 — Coiffure (Owner 2)
```
POST https://localhost:6001/shops/admin/create-for-owner
Content-Type: application/json

{
  "ownerId": "dc433a93-f507-4d48-bbb5-684a4ffaff62",
  "name": "Salon Belle Coupe",
  "vatNumber": "BE0678912345",
  "type": "Physical",
  "status": "Open",
  "category": "Coiffure",
  "promoStrategy": "BestForClient",
  "address": {
    "firstLine": "Rue Léopold 17",
    "city": "Liège",
    "state": "Liège",
    "postalCode": "4000",
    "country": "Belgium"
  }
}
```

---

## 2. CRÉER LES PRODUITS

### Boulangerie Martin (shopId: 2)
```
POST https://localhost:6001/products/shop/2
Content-Type: application/json

{
  "name": "Baguette tradition",
  "description": "Baguette artisanale au levain",
  "sellPrice": 1.20,
  "buyPrice": 0.40,
  "unitOfPrice": "PerUnit",
  "codeBar": "3760001234561",
  "volume": 0,
  "unitOfVolume": "",
  "weight": 250
}
```

```
POST https://localhost:6001/products/shop/2
Content-Type: application/json

{
  "name": "Croissant beurre",
  "description": "Croissant pur beurre AOP",
  "sellPrice": 1.50,
  "buyPrice": 0.55,
  "unitOfPrice": "PerUnit",
  "codeBar": "3760001234562",
  "volume": 0,
  "unitOfVolume": "",
  "weight": 80
}
```

```
POST https://localhost:6001/products/shop/2
Content-Type: application/json

{
  "name": "Pain complet",
  "description": "Pain complet aux céréales",
  "sellPrice": 2.80,
  "buyPrice": 0.90,
  "unitOfPrice": "PerUnit",
  "codeBar": "3760001234563",
  "volume": 0,
  "unitOfVolume": "",
  "weight": 400
}
```

```
POST https://localhost:6001/products/shop/2
Content-Type: application/json

{
  "name": "Tarte aux fruits",
  "description": "Tarte aux fruits frais de saison",
  "sellPrice": 18.00,
  "buyPrice": 7.00,
  "unitOfPrice": "PerUnit",
  "codeBar": "3760001234564",
  "volume": 0,
  "unitOfVolume": "",
  "weight": 600
}
```

### Fromagerie Dubois (shopId: 3)
```
POST https://localhost:6001/products/shop/3
Content-Type: application/json

{
  "name": "Comté 24 mois",
  "description": "Comté affiné 24 mois AOP",
  "sellPrice": 28.50,
  "buyPrice": 18.00,
  "unitOfPrice": "Kg",
  "codeBar": "3760009876541",
  "volume": 0,
  "unitOfVolume": "",
  "weight": 1000
}
```

```
POST https://localhost:6001/products/shop/3
Content-Type: application/json

{
  "name": "Brie de Meaux",
  "description": "Brie de Meaux AOP",
  "sellPrice": 22.00,
  "buyPrice": 14.00,
  "unitOfPrice": "Kg",
  "codeBar": "3760009876542",
  "volume": 0,
  "unitOfVolume": "",
  "weight": 500
}
```

```
POST https://localhost:6001/products/shop/3
Content-Type: application/json

{
  "name": "Roquefort",
  "description": "Roquefort AOP affiné en cave",
  "sellPrice": 32.00,
  "buyPrice": 20.00,
  "unitOfPrice": "Kg",
  "codeBar": "3760009876543",
  "volume": 0,
  "unitOfVolume": "",
  "weight": 250
}
```

### Librairie Lecomte (shopId: 4)
```
POST https://localhost:6001/products/shop/4
Content-Type: application/json

{
  "name": "Roman policier",
  "description": "Dernier bestseller policier",
  "sellPrice": 19.90,
  "buyPrice": 10.00,
  "unitOfPrice": "PerUnit",
  "codeBar": "9782070360021",
  "volume": 0,
  "unitOfVolume": "",
  "weight": 320
}
```

```
POST https://localhost:6001/products/shop/4
Content-Type: application/json

{
  "name": "BD Tintin",
  "description": "Collection Tintin - Le Lotus Bleu",
  "sellPrice": 12.50,
  "buyPrice": 6.00,
  "unitOfPrice": "PerUnit",
  "codeBar": "9782070360022",
  "volume": 0,
  "unitOfVolume": "",
  "weight": 150
}
```

```
POST https://localhost:6001/products/shop/4
Content-Type: application/json

{
  "name": "Cahier A4",
  "description": "Cahier 200 pages grands carreaux",
  "sellPrice": 3.50,
  "buyPrice": 1.20,
  "unitOfPrice": "PerUnit",
  "codeBar": "9782070360023",
  "volume": 0,
  "unitOfVolume": "",
  "weight": 200
}
```

### Boucherie Renard (shopId: 5)
```
POST https://localhost:6001/products/shop/5
Content-Type: application/json

{
  "name": "Entrecôte",
  "description": "Entrecôte de boeuf belge",
  "sellPrice": 24.90,
  "buyPrice": 14.00,
  "unitOfPrice": "Kg",
  "codeBar": "3760005551111",
  "volume": 0,
  "unitOfVolume": "",
  "weight": 1000
}
```

```
POST https://localhost:6001/products/shop/5
Content-Type: application/json

{
  "name": "Poulet fermier",
  "description": "Poulet fermier label rouge",
  "sellPrice": 12.90,
  "buyPrice": 7.50,
  "unitOfPrice": "Kg",
  "codeBar": "3760005551112",
  "volume": 0,
  "unitOfVolume": "",
  "weight": 1500
}
```

```
POST https://localhost:6001/products/shop/5
Content-Type: application/json

{
  "name": "Saucisse de Liège",
  "description": "Saucisse de Liège artisanale",
  "sellPrice": 8.50,
  "buyPrice": 4.00,
  "unitOfPrice": "PerUnit",
  "codeBar": "3760005551113",
  "volume": 0,
  "unitOfVolume": "",
  "weight": 300
}
```

### TechZone (shopId: 6)
```
POST https://localhost:6001/products/shop/6
Content-Type: application/json

{
  "name": "Samsung Galaxy S24",
  "description": "Smartphone Samsung Galaxy S24 128Go",
  "sellPrice": 899.00,
  "buyPrice": 650.00,
  "unitOfPrice": "PerUnit",
  "codeBar": "8806094913583",
  "volume": 0,
  "unitOfVolume": "",
  "weight": 167
}
```

```
POST https://localhost:6001/products/shop/6
Content-Type: application/json

{
  "name": "AirPods Pro",
  "description": "Apple AirPods Pro 2ème génération",
  "sellPrice": 279.00,
  "buyPrice": 190.00,
  "unitOfPrice": "PerUnit",
  "codeBar": "0194253714057",
  "volume": 0,
  "unitOfVolume": "",
  "weight": 61
}
```

```
POST https://localhost:6001/products/shop/6
Content-Type: application/json

{
  "name": "Clé USB 64Go",
  "description": "Clé USB 3.0 SanDisk 64Go",
  "sellPrice": 14.99,
  "buyPrice": 7.00,
  "unitOfPrice": "PerUnit",
  "codeBar": "0619659161729",
  "volume": 0,
  "unitOfVolume": "",
  "weight": 10
}
```

---

## 3. TESTER LES FILTRES SHOPS

```
GET https://localhost:6001/shops?category=Boulangerie
GET https://localhost:6001/shops?category=Multimedia
GET https://localhost:6001/shops?category=Boucherie
GET https://localhost:6001/shops?category=Coiffure
GET https://localhost:6001/shops?city=Liège
GET https://localhost:6001/shops?status=Open
GET https://localhost:6001/shops?type=Physical
GET https://localhost:6001/shops/open-now
```

---

## 4. TESTER LES FILTRES PRODUCTS

```
GET https://localhost:6001/products?shopId=2
GET https://localhost:6001/products?shopId=5&sortBy=PriceDesc
GET https://localhost:6001/products?minPrice=10&maxPrice=30
GET https://localhost:6001/products?minPrice=1&maxPrice=5
GET https://localhost:6001/products/search?q=baguette
GET https://localhost:6001/products/search?q=samsung
GET https://localhost:6001/products/search?q=3760001234561
GET https://localhost:6001/products?hasActivePromotion=true
```

---

## 5. TESTER LES PROMOTIONS

### Créer une promo sur la boulangerie
```
POST https://localhost:6001/promotions/shop/2
Content-Type: application/json

{
  "name": "Soldes été boulangerie",
  "description": "10% sur tous les produits",
  "type": "Percentage",
  "discountPercentage": 10,
  "startDate": "2026-02-18T00:00:00",
  "endDate": "2026-12-31T00:00:00",
  "productIds": [1, 2, 3],
  "isActive": true
}
```

### Créer une promo directement sur un produit
```
POST https://localhost:6001/products/4/promotions/create
Content-Type: application/json

{
  "name": "3 croissants achetés 1 offert",
  "type": "ForXGetY",
  "buyQuantity": 3,
  "getQuantity": 1,
  "startDate": "2026-02-18T00:00:00",
  "endDate": "2026-12-31T00:00:00",
  "isActive": true
}
```

### Simuler des prix
```
GET https://localhost:6001/promotions/1/simulate?price=1.20&quantity=5
GET https://localhost:6001/promotions/1/simulate?price=1.50&quantity=4
GET https://localhost:6001/promotions/2/simulate?price=1.50&quantity=4
```

---

## 6. TESTER LA COPIE DE PRODUIT

```
POST https://localhost:6001/products/1/copy-to/3
POST https://localhost:6001/products/5/copy-to/3
```

---

## 7. TESTER MODIFIER / SUPPRIMER

```
PUT https://localhost:6001/shops/2
Content-Type: application/json

{
  "name": "Boulangerie Martin & Fils",
  "status": "Open"
}
```

```
PUT https://localhost:6001/products/1
Content-Type: application/json

{
  "name": "Baguette tradition premium",
  "sellPrice": 1.40
}
```

```
DELETE https://localhost:6001/products/3
DELETE https://localhost:6001/shops/6
```
