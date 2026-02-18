# API Endpoints Documentation

Base URL: `https://localhost:6001`

---

## Account

| Méthode | URL | Rôle | Description |
|---|---|---|---|
| POST | `/account/register` | Tout le monde | Créer un compte |
| POST | `/account/login` | Tout le monde | Se connecter |
| POST | `/account/logout` | Connecté | Se déconnecter |
| GET | `/account/my-role` | Connecté | Voir son propre rôle |
| GET | `/account/get-user?Email=owner@test.com` | Admin | Récupérer un user par email ou id |
| GET | `/account/all` | Admin | Lister tous les users |
| PUT | `/account/change-role` | Admin | Changer le rôle d'un user |
| PUT | `/account/update-user` | Connecté | Modifier son profil (admin peut modifier n'importe qui) |
| DELETE | `/account/delete` | Admin | Supprimer un user |
| DELETE | `/account/delete-all` | Admin | Supprimer tous les users |

---

## Shops

| Méthode | URL | Rôle | Description |
|---|---|---|---|
| GET | `/shops` | Tout le monde | Lister tous les shops avec filtres et pagination — filtres disponibles : `name`, `city`, `country`, `status` (Open/Closed/NotDefined), `type` (Physical/Online/Both)`, `pageIndex`, `pageSize` — ex: `/shops?city=Liège&status=Open&pageIndex=1&pageSize=10` |
| GET | `/shops/open-now` | Tout le monde | Lister les shops ouverts en ce moment |
| GET | `/shops/{id}/store` | Tout le monde | Voir les détails publics d'un shop |
| GET | `/shops/my-shops` | Owner | Voir ses propres shops |
| GET | `/shops/by-owner/{ownerId}` | Admin | Voir les shops d'un owner spécifique |
| GET | `/shops/{id}` | Admin, Owner | Voir les détails complets d'un shop |
| POST | `/shops` | Owner | Créer un shop pour soi-même |
| POST | `/shops/admin/create-for-owner` | Admin | Créer un shop pour un owner spécifique |
| PUT | `/shops/{id}` | Admin, Owner | Modifier un shop |
| DELETE | `/shops/{id}` | Admin, Owner | Supprimer un shop |

---

## Exemples de Body

### POST /account/register
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "owner@test.com",
  "password": "Test123",
  "phoneNumber": "0123456789",
  "role": "owner"
}
```

### POST /account/login
```json
{
  "email": "owner@test.com",
  "password": "Test123"
}
```

### PUT /account/change-role
```json
{
  "email": "owner@test.com",
  "role": "Client"
}
```

### PUT /account/update-user
```json
{
  "firstName": "Jane",
  "lastName": "Smith",
  "phoneNumber": "0987654321"
}
```

### DELETE /account/delete
```json
{
  "email": "owner@test.com"
}
```

### POST /shops
```json
{
  "name": "Ma Boulangerie",
  "vatNumber": "BE0123456789",
  "type": "Physical",
  "status": "Open",
  "promoStrategy": "BestForClient"
}
```

### POST /shops/admin/create-for-owner
```json
{
  "ownerId": "86f62e43-aa84-44d5-a195-20439d69aa85",
  "name": "Shop Admin",
  "vatNumber": "BE0999999999",
  "type": "Physical",
  "status": "Open",
  "promoStrategy": "BestForClient"
}
```

### PUT /shops/{id}
```json
{
  "name": "Boulangerie Martin Modifiée",
  "status": "Closed"
}
```

---

## Query Params disponibles pour GET /shops

| Param | Description | Exemple |
|---|---|---|
| `name` | Filtrer par nom | `?name=Boulangerie` |
| `city` | Filtrer par ville | `?city=Liège` |
| `country` | Filtrer par pays | `?country=Belgium` |
| `status` | Filtrer par statut | `?status=Open` |
| `type` | Filtrer par type | `?type=Physical` |
| `ownerId` | Filtrer par owner | `?ownerId=86f62e43...` |
| `pageIndex` | Numéro de page | `?pageIndex=1` |
| `pageSize` | Taille de page | `?pageSize=10` |
