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
| POST | `/auth/register` | `{ email, password }` | Hashes password (BCrypt), creates Account, returns `AuthResponseDto` (`{ token, account }`) — auto-login on register. Also sets the refresh-token cookie (see below). |
| POST | `/auth/login` | `{ email, password }` | Verifies password, returns `AuthResponseDto` (`{ token, account }`). Nonexistent email and wrong password both return an identical `401` — no distinguishing info, to avoid account enumeration. Also sets the refresh-token cookie. |
| POST | `/auth/refresh` | — (reads the `refreshToken` cookie) | Exchanges a valid, unexpired, non-revoked refresh token for a fresh access token, rotating the refresh token in the process (old one revoked, new one set as the cookie). `401` if the cookie is missing, expired, or already revoked — reuse of an already-revoked token revokes every active refresh token on the account as a stolen-token defense. Called automatically on app load so a page refresh doesn't log the user out. Returns `AuthResponseDto`. |
| POST | `/auth/logout` | — | Revokes the current refresh token (if the cookie is present) and clears it. Returns `204 No Content`. |

## Lookup (reference data)

| Method | Route | Notes |
|---|---|---|
| GET | `/lookup/spirits` | **Requires auth** (not admin-only — any signed-in user needs this to render preference-selection UI). Every `Spirit`, `LookupItemDto[]` (`{ id, name }`), alphabetical. |
| GET | `/lookup/flavor-tags` | Same shape, every `FlavorTag`. |

## Profiles

| Method | Route | Body | Notes |
|---|---|---|---|
| GET | `/profiles` | — | **Requires auth.** Lists profiles under the authenticated account — account identified via the JWT's `sub` claim (see `ProfilesController.GetAccountId()`), not a route param or request body. Projected through `ProfileDto` (id/displayName/avatarColor/createdAt). |
| POST | `/profiles` | `{ displayName, avatarColor? }` | **Requires auth.** Creates a profile under the authenticated account. `avatarColor` is optional, defaults to `"gray"` server-side if omitted. `accountId` is deliberately *not* part of the request body — it always comes from the token, never from client-supplied data, so one account can never create a profile under another. |
| GET | `/profiles/{id}/preferences` | — | **Requires auth + ownership.** Returns spirit preferences, flavor preferences, and allergens for the given profile — `ProfilePreferencesDto`, always real (possibly empty) lists, never `null`. `404` if the profile doesn't exist *or* belongs to a different account — same non-distinguishing response either way, same reasoning as Login's identical failure for both wrong-password and no-such-email. |
| PUT | `/profiles/{id}/preferences` | `UpdateProfilePreferencesDto` (`{ spiritPreferences, flavorPreferences, allergens }`, ids not names) | **Requires auth + ownership.** Wholesale replace, not a diff/patch — deletes every existing preference/allergen row for the profile, then inserts the submitted set. Returns `204 No Content` (a full round-trip `GET` after a successful `PUT` would need to re-query for spirit/flavor names anyway, since the request only carries ids). |
| GET | `/profiles/{id}/recommendations` | — | **Requires auth + ownership**, same `404` semantics as above. Runs `TasteRankingService.RankCocktailsForProfileAsync`: every cocktail scored and sorted for this profile, `CocktailSummaryDto[]` (includes the signed `matchScore` field — see below). Cocktails matching an allergen are excluded entirely, not scored — see `DATA_MODEL.md` "Taste ranking" for the full scoring design. A profile with no preferences set returns every cocktail at `matchScore: 0`, alphabetical — never empty or an error. |
| GET | `/profiles/{id}/cookbook` | **Requires auth + ownership.** Every saved `CookbookEntry` for the profile, newest first, `CookbookEntryDto[]`. |
| POST | `/profiles/{id}/cookbook` | `{ cocktailId, notes? }` | **Requires auth + ownership.** Upserts on `(profileId, cocktailId)` — saving an already-saved cocktail updates `notes` instead of duplicating or erroring. `404` if the cocktail doesn't exist. Returns `204 No Content`. |
| DELETE | `/profiles/{id}/cookbook/{cocktailId}` | — | **Requires auth + ownership.** `404` if the profile doesn't own the cocktail-saving-entry (covers both "never saved" and "not this profile's entry"). Returns `204 No Content`. |

## Cocktails / Discovery

| Method | Route | Notes |
|---|---|---|
| GET | `/cocktails?search=&category=&seasons=&flavorTags=&spirits=&profileId=` | **Requires auth.** Browse/search local cache. All filters optional and combinable (AND across filter types); `search` matches partial, case-insensitive `Name` (`.ToLower().Contains()` — Postgres string comparison is case-sensitive by default, so both sides get lowercased); `category` is an exact match. `seasons`/`flavorTags`/`spirits` are comma-separated (e.g. `seasons=Summer,Winter`, same parsing style as `what-can-i-make`'s `ingredients`) — multi-select, OR within one filter (any selected value matches) combined with AND across filters (e.g. "Summer or Winter" AND "Sweet or Citrus" AND "Rum or Vodka"). `spirits` matches via `CocktailIngredient → Ingredient.SpiritId → Spirit.Name`, same link `IngredientSpiritSyncService` populates for taste ranking. `profileId` is optional but must belong to the authenticated account (`404` otherwise, same non-distinguishing response as other ownership checks) — when given, results are scored and ordered by `TasteRankingService` (highest `matchScore` first, alphabetical tiebreak) and allergen-matching cocktails are excluded entirely; omitted, every result scores `0` and the list is effectively alphabetical. Projected through `CocktailSummaryDto` (id/name/category/glass/imageUrl/flavorTags/matchScore — no instructions/ingredients, kept lean for list views). |
| GET | `/cocktails/{id}` | **Requires auth.** Full recipe detail — `CocktailDetailDto` (summary fields + instructions + full ingredient list with measures). `404` if the id doesn't exist. Ingredients loaded via `.Include()`/`.ThenInclude()` eager loading. |
| GET | `/cocktails/browse?perCategory=4&profileId=` | **Requires auth.** One row per real `Category` value, each with up to `perCategory` cocktails — powers the home page's Netflix-style category rails in a single request instead of one call per category. Same `profileId` semantics as `/cocktails` above: each category's cocktails are scored and ranked *before* truncating to `perCategory`, so "top 4" means top 4 by match score (falling back to alphabetical with no `profileId`), not top 4 alphabetically re-sorted after the fact. `CategoryPreviewDto[]` (`{ category, cocktails: CocktailSummaryDto[] }`). |
| GET | `/cocktails/what-can-i-make?ingredients=vodka,lime,mint` | **Requires auth.** Comma-separated ingredient names the caller has on hand, matched case-insensitively against real `Ingredient.Name` values by *exact* match, not fuzzy/substring — "what can I make" is an inventory check, and a fuzzy match risks telling someone they can make a drink they can't. Only cocktails with at least one matching ingredient are returned (otherwise all 426 would come back, mostly irrelevant); ranked ascending by how many ingredients are still missing, so fully-makeable cocktails (`missingIngredients: []`) sort first. `WhatCanIMakeResultDto[]`. |

Cookbook endpoints are documented under **Profiles** above (`/profiles/{id}/cookbook`) since they're profile-scoped like preferences/recommendations — the route shape settled during implementation as `DELETE .../cookbook/{cocktailId}` rather than `{entryId}`, since a frontend "unsave" action only ever knows the cocktail id, not the `CookbookEntry`'s own internal id.

## Substitution

| Method | Route | Body | Notes |
|---|---|---|---|
| POST | `/substitutions/suggest` | `{ cocktailId, ingredientId, reason: "Taste"|"Availability", subReason?: "Cost"|"Supply" }` | **Requires auth.** `400` if `reason`/`subReason` don't form a valid combination. `404` if the ingredient isn't actually part of that cocktail, or if no `IngredientSubstitution` rule exists for the resolved reason. Returns `SubstitutionSuggestionDto` (`{ replacementIngredientId, replacementIngredientName, notes }`). |

## Admin / sync

All routes below **require the `Admin` role**, not just being logged in (`[Authorize(Roles = "Admin")]`). `Account.IsAdmin` (added 2026-09-28) drives a `"role": "Admin"` claim baked into the JWT at login — see `ARCHITECTURE.md` for how an account becomes an admin and the security reasoning behind it. A logged-in non-admin account gets `403 Forbidden`; no token at all gets `401 Unauthorized`.

| Method | Route | Notes |
|---|---|---|
| POST | `/admin/sync-cocktails` | Runs `CocktailDbSyncService`: walks TheCocktailDB's `search.php?f=<letter>` for a–z, adds any cocktails not already synced (matched by `ExternalId`), dedupes ingredients by name, and assigns seasons via `SeasonHeuristic`. Safe to re-run — already-synced cocktails are skipped. Returns `{ cocktailsAdded }`. |
| POST | `/admin/tag-ingredient-flavors` | Runs `IngredientFlavorTagSyncService`: wholesale-replaces `IngredientFlavorTags` by re-running `IngredientFlavorHeuristic` against every `Ingredient`. Safe to re-run after keyword-list changes — always reflects the current heuristic, never accumulates stale tags. Returns `{ tagsAdded }`. |
| POST | `/admin/tag-cocktail-flavors` | Runs `CocktailFlavorTagSyncService`: wholesale-replaces `CocktailFlavorTags` using the "half of max" prominence rule over each cocktail's `IngredientFlavorTags` (see `DATA_MODEL.md`). Depends on ingredient-level tagging already being up to date — re-run `tag-ingredient-flavors` first if keyword lists changed. Returns `{ tagsAdded }`. |
| POST | `/admin/identify-ingredient-spirits` | Runs `IngredientSpiritSyncService`: sets (or clears) `Ingredient.SpiritId` for every ingredient via `IngredientSpiritHeuristic`. Safe to re-run — always reflects the current heuristic. Returns `{ ingredientsMatched }`. |
| POST | `/admin/seed-ingredient-substitutions` | Runs `IngredientSubstitutionSeedService`: adds any curated substitution rules not already present (per-rule existence check, not a wholesale replace — safe to re-run after adding new curated rules later). Returns `{ rulesAdded }`. |

**Sync/tagging order matters on a fresh database:** `sync-cocktails` first (creates `Ingredient`/`Cocktail` rows), then `tag-ingredient-flavors` and `identify-ingredient-spirits` (either order, both only depend on `Ingredient`), then `tag-cocktail-flavors` last (depends on ingredient-level flavor tags already existing).

## Conventions

- JSON in, JSON out. `camelCase` field names on the wire (ASP.NET Core's default JSON serializer handles the PascalCase-C#-to-camelCase-JSON conversion automatically).
- Errors: standard HTTP status codes + a `{ message }` body. We'll formalize a `ProblemDetails`-based error shape once we hit a case that needs it, rather than over-designing error handling before we have a real error to handle.
- Pagination: not in MVP scope — TheCocktailDB result sets are small enough that we return full lists. Revisit if the local cache grows large enough to matter.
