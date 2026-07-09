# Proof — API Design

Companion to [DATA_MODEL.md](./DATA_MODEL.md). Endpoints are added incrementally per [ROADMAP.md](./ROADMAP.md) phase — this doc tracks what actually exists, not the full aspirational surface. Entries marked *(planned)* haven't been built yet.

Base URL (dev): `http://localhost:5xxx/api`

All endpoints except `/auth/*` require `Authorization: Bearer <jwt>`.

## Auth

| Method | Route | Body | Notes |
|---|---|---|---|
| POST | `/auth/register` | `{ email, password }` | Creates Account, returns JWT |
| POST | `/auth/login` | `{ email, password }` | Returns JWT |

## Profiles

| Method | Route | Notes |
|---|---|---|
| GET | `/profiles` | List profiles under the authenticated account |
| POST | `/profiles` | Create a profile |
| GET | `/profiles/{id}/preferences` | Get taste preference profile |
| PUT | `/profiles/{id}/preferences` | Update spirits/flavors/allergens |

## Cocktails / Discovery *(planned)*

| Method | Route | Notes |
|---|---|---|
| GET | `/cocktails?search=&category=&season=&profileId=` | Browse/search, ranked by profile if `profileId` given |
| GET | `/cocktails/{id}` | Full recipe detail |
| GET | `/cocktails/what-can-i-make?ingredients=vodka,lime,mint` | Local-cache subset-match filter |

## Cookbook *(planned)*

| Method | Route | Notes |
|---|---|---|
| GET | `/profiles/{profileId}/cookbook` | List saved recipes |
| POST | `/profiles/{profileId}/cookbook` | Save a recipe (with optional substitutions) |
| DELETE | `/profiles/{profileId}/cookbook/{entryId}` | Remove a saved recipe |

## Substitution *(planned)*

| Method | Route | Body | Notes |
|---|---|---|---|
| POST | `/substitutions/suggest` | `{ cocktailId, ingredientId, reason: "Taste"|"Availability", subReason?: "Cost"|"Supply" }` | Returns a suggested replacement ingredient per the two-question flow |

## Admin / sync *(planned)*

| Method | Route | Notes |
|---|---|---|
| POST | `/admin/sync-cocktails` | Triggers the TheCocktailDB → local cache sync job |

## Conventions

- JSON in, JSON out. `camelCase` field names on the wire (ASP.NET Core's default JSON serializer handles the PascalCase-C#-to-camelCase-JSON conversion automatically).
- Errors: standard HTTP status codes + a `{ message }` body. We'll formalize a `ProblemDetails`-based error shape once we hit a case that needs it, rather than over-designing error handling before we have a real error to handle.
- Pagination: not in MVP scope — TheCocktailDB result sets are small enough that we return full lists. Revisit if the local cache grows large enough to matter.
