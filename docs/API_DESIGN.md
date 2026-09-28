# Proof — API Design

Companion to [DATA_MODEL.md](./DATA_MODEL.md). Endpoints are added incrementally per [ROADMAP.md](./ROADMAP.md) phase — this doc tracks what actually exists, not the full aspirational surface. Entries marked *(planned)* haven't been built yet.

Base URL (dev): `http://localhost:5xxx/api`

All endpoints except `/auth/*` require `Authorization: Bearer <jwt>`.

## Accounts

| Method | Route | Notes |
|---|---|---|
| GET | `/accounts` | **Requires auth.** Lists accounts, projected through `AccountDto` (id/email/createdAt only — `PasswordHash` never leaves the server). Built in Phase 1 as the first end-to-end slice; protected with `[Authorize]` once JWT auth landed in Phase 3. |

## Auth

| Method | Route | Body | Notes |
|---|---|---|---|
| POST | `/auth/register` | `{ email, password }` | Hashes password (BCrypt), creates Account, returns `AuthResponseDto` (`{ token, account }`) — auto-login on register |
| POST | `/auth/login` | `{ email, password }` | Verifies password, returns `AuthResponseDto` (`{ token, account }`). Nonexistent email and wrong password both return an identical `401` — no distinguishing info, to avoid account enumeration |

## Profiles

| Method | Route | Body | Notes |
|---|---|---|---|
| GET | `/profiles` | — | **Requires auth.** Lists profiles under the authenticated account — account identified via the JWT's `sub` claim (see `ProfilesController.GetAccountId()`), not a route param or request body. Projected through `ProfileDto` (id/displayName/avatarColor/createdAt). |
| POST | `/profiles` | `{ displayName, avatarColor? }` | **Requires auth.** Creates a profile under the authenticated account. `avatarColor` is optional, defaults to `"gray"` server-side if omitted. `accountId` is deliberately *not* part of the request body — it always comes from the token, never from client-supplied data, so one account can never create a profile under another. |
| GET | `/profiles/{id}/preferences` | — | **Requires auth + ownership.** Returns spirit preferences, flavor preferences, and allergens for the given profile — `ProfilePreferencesDto`, always real (possibly empty) lists, never `null`. `404` if the profile doesn't exist *or* belongs to a different account — same non-distinguishing response either way, same reasoning as Login's identical failure for both wrong-password and no-such-email. |
| PUT | `/profiles/{id}/preferences` | `UpdateProfilePreferencesDto` (`{ spiritPreferences, flavorPreferences, allergens }`, ids not names) | **Requires auth + ownership.** Wholesale replace, not a diff/patch — deletes every existing preference/allergen row for the profile, then inserts the submitted set. Returns `204 No Content` (a full round-trip `GET` after a successful `PUT` would need to re-query for spirit/flavor names anyway, since the request only carries ids). |

## Cocktails / Discovery

| Method | Route | Notes |
|---|---|---|
| GET | `/cocktails?search=&category=&season=` | **Requires auth.** Browse/search local cache. All three filters optional and combinable; `search` matches partial, case-insensitive `Name` (`.ToLower().Contains()` — Postgres string comparison is case-sensitive by default, so both sides get lowercased); `category` is an exact match; `season` matches cocktails tagged with that `Season` value via `CocktailSeasons`. Projected through `CocktailSummaryDto` (id/name/category/glass/imageUrl — no instructions/ingredients, kept lean for list views). Profile-based ranking (`profileId`) deferred to Phase 6, same as taste preferences. |
| GET | `/cocktails/{id}` | **Requires auth.** Full recipe detail — `CocktailDetailDto` (summary fields + instructions + full ingredient list with measures). `404` if the id doesn't exist. Ingredients loaded via `.Include()`/`.ThenInclude()` eager loading. |
| GET | `/cocktails/what-can-i-make?ingredients=vodka,lime,mint` *(planned — Phase 9)* | Local-cache subset-match filter |

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

## Admin / sync

All routes below **require the `Admin` role**, not just being logged in (`[Authorize(Roles = "Admin")]`). `Account.IsAdmin` (added 2026-09-28) drives a `"role": "Admin"` claim baked into the JWT at login — see `ARCHITECTURE.md` for how an account becomes an admin and the security reasoning behind it. A logged-in non-admin account gets `403 Forbidden`; no token at all gets `401 Unauthorized`.

| Method | Route | Notes |
|---|---|---|
| POST | `/admin/sync-cocktails` | Runs `CocktailDbSyncService`: walks TheCocktailDB's `search.php?f=<letter>` for a–z, adds any cocktails not already synced (matched by `ExternalId`), dedupes ingredients by name, and assigns seasons via `SeasonHeuristic`. Safe to re-run — already-synced cocktails are skipped. Returns `{ cocktailsAdded }`. |
| POST | `/admin/tag-ingredient-flavors` | Runs `IngredientFlavorTagSyncService`: wholesale-replaces `IngredientFlavorTags` by re-running `IngredientFlavorHeuristic` against every `Ingredient`. Safe to re-run after keyword-list changes — always reflects the current heuristic, never accumulates stale tags. Returns `{ tagsAdded }`. |
| POST | `/admin/tag-cocktail-flavors` | Runs `CocktailFlavorTagSyncService`: wholesale-replaces `CocktailFlavorTags` using the "half of max" prominence rule over each cocktail's `IngredientFlavorTags` (see `DATA_MODEL.md`). Depends on ingredient-level tagging already being up to date — re-run `tag-ingredient-flavors` first if keyword lists changed. Returns `{ tagsAdded }`. |

## Conventions

- JSON in, JSON out. `camelCase` field names on the wire (ASP.NET Core's default JSON serializer handles the PascalCase-C#-to-camelCase-JSON conversion automatically).
- Errors: standard HTTP status codes + a `{ message }` body. We'll formalize a `ProblemDetails`-based error shape once we hit a case that needs it, rather than over-designing error handling before we have a real error to handle.
- Pagination: not in MVP scope — TheCocktailDB result sets are small enough that we return full lists. Revisit if the local cache grows large enough to matter.
