# Proof — Data Model

Companion to [ARCHITECTURE.md](./ARCHITECTURE.md). This describes entities and relationships conceptually; actual EF Core classes will match this but may gain fields as we build (this doc should be updated when they do).

## Entity-relationship summary

```
Account 1---* Profile

Profile 1---* ProfileSpiritPreference *---1 Spirit
Profile 1---* ProfileFlavorPreference *---1 FlavorTag
Profile 1---* ProfileAllergen

Ingredient *---* FlavorTag        (via IngredientFlavorTag)
Ingredient 1---* IngredientSubstitution (as Source)
Ingredient 1---* IngredientSubstitution (as Replacement)

Cocktail 1---* CocktailIngredient *---1 Ingredient
Cocktail 1---* CocktailSeason
Cocktail *---* FlavorTag          (via CocktailFlavorTag — derived from ingredient tags, see design note)
Cocktail *---1 Profile            (OwnerProfileId, nullable — only set for custom recipes)

Profile 1---* CookbookEntry *---1 Cocktail
CookbookEntry 1---* CookbookEntrySubstitution
```

## Entities

### Account
Login identity. Not tied to taste/preferences directly.

| Field | Type | Notes |
|---|---|---|
| Id | Guid/int | PK |
| Email | string | unique |
| PasswordHash | string | BCrypt hash, never the raw password |
| CreatedAt | timestamp | |

### Profile
A sub-user under an Account (like a Netflix profile). This is the entity everything taste-related hangs off of.

| Field | Type | Notes |
|---|---|---|
| Id | Guid/int | PK |
| AccountId | FK → Account | |
| DisplayName | string | |
| AvatarColor | string | small personality touch, no image upload needed for MVP |
| CreatedAt | timestamp | |

### Spirit (lookup table)
Reference list: bourbon, gin, rum, tequila, vodka, whiskey, etc. Seeded data, not user-editable.

**`Ingredient.SpiritId` (added 2026-09-28)** — nullable FK, `Ingredient → Spirit`. Closes a real gap found while designing taste ranking: `ProfileSpiritPreference` links to the clean 12-entry `Spirit` table, but a cocktail's actual ingredients are `Ingredient` rows with free-form names (`"Coconut rum"`, `"Jim Beam"`, `"Absolut Citron"`) — without this link there'd be no way to connect a cocktail containing those to a profile's stated Rum/Bourbon/Vodka preference, which would make spirit preferences nearly useless for ranking. Populated by `IngredientSpiritSyncService` running `IngredientSpiritHeuristic` (same keyword-matching mechanism as `IngredientFlavorHeuristic`, factored into a shared `IngredientKeywordMatcher` since it's now used twice) — a single nullable best match per ingredient, not a set, since an ingredient can only be one spirit at most. Specific spirits (Bourbon, Scotch, Cognac) are checked before generic buckets they could fall into (Whiskey, Brandy) so brand names map to the more specific category (e.g. "Jim Beam" → Bourbon, not Whiskey). Verified against real data: 64 of 327 ingredients matched.

### ProfileSpiritPreference
| Field | Notes |
|---|---|
| ProfileId | FK |
| SpiritId | FK |
| Sentiment | enum: `Likes` / `Dislikes` |

### FlavorTag (lookup table)
Reference list: sweet, sour, bitter, citrus, herbal, spicy, smoky, floral, etc. Also used to tag `Ingredient` rows, so this table is shared between profile preferences and ingredient characteristics — that shared vocabulary is what makes taste-based ranking possible (Phase 6).

### ProfileFlavorPreference
Same shape as ProfileSpiritPreference: ProfileId, FlavorTagId, Sentiment (`Prefers` / `Avoids`).

### ProfileAllergen
| Field | Notes |
|---|---|
| ProfileId | FK |
| Name | freetext (e.g. "dairy", "tree nuts") — no fixed lookup list; allergens are too varied to enumerate up front |

### Ingredient
Every ingredient that appears in any cocktail — spirits, mixers, garnishes, bitters, syrups. This is the backbone of both substitution and "What Can I Make?".

| Field | Notes |
|---|---|
| Id | PK |
| ExternalId | TheCocktailDB ingredient id, nullable (null for anything we add manually) |
| Name | |
| Type | enum: Spirit / Mixer / Garnish / Bitters / Syrup / Other |
| CostTier | enum: Budget / Mid / Splurge — drives cost-swap substitution |
| AvailabilityTier | enum: Common / Specialty / RareHardToFind — drives supply-swap substitution |

### IngredientFlavorTag (join)
Ingredient ↔ FlavorTag, many-to-many. E.g. "Angostura bitters" tags to `bitter` and `herbal`.

### Cocktail
| Field | Notes |
|---|---|
| Id | PK |
| ExternalId | TheCocktailDB id, nullable |
| Name | |
| Category | e.g. "Cocktail", "Shot", "Punch / Party Drink" (from source data, with one normalization — see below) |
| Glass | |
| Instructions | |
| ImageUrl | |
| IsCustom | bool — true for user-authored recipes (Phase-2 feature, model built now) |
| OwnerProfileId | FK → Profile, nullable — only set when IsCustom is true |

### CocktailIngredient (join)
Cocktail ↔ Ingredient, many-to-many, with per-recipe detail.

| Field | Notes |
|---|---|
| CocktailId | FK |
| IngredientId | FK |
| Measure | freetext, e.g. "1 1/2 oz" (matches source data format) |
| SortOrder | preserves ingredient list order from the recipe |

### CocktailSeason (join)
Cocktail ↔ Season, many-to-many — a cocktail can belong to multiple seasons (e.g. a Moscow Mule fits both Spring and Summer; a warm spiced drink can fit both Fall and Winter). TheCocktailDB has no season data at all, so this is populated by a heuristic we write ourselves during sync (see `ARCHITECTURE.md`), not sourced data.

| Field | Notes |
|---|---|
| CocktailId | FK |
| Season | enum: `Spring` / `Summer` / `Fall` / `Winter` |

### CocktailFlavorTag (join)
Cocktail ↔ FlavorTag, many-to-many — a cocktail's *own* flavor profile, distinct from `IngredientFlavorTag` (which tags individual ingredients). Added 2026-08-22, Brian's design call: this is deliberately *not* the union of every ingredient's flavor tags — that would be noisy and often self-contradictory (a mostly-sweet drink with one ingredient added just to balance it shouldn't read as both "Sweet" and "Sour"). Populated by `CocktailFlavorTagSyncService.TagAllCocktailsAsync()` (finished 2026-09-28), which runs after ingredient-level tagging exists. Same underlying mechanism applies automatically to future custom recipes (Phase-2 feature) — no manual curation needed, since it's fully computed from whatever ingredients a recipe lists.

**Prominence rule (the "half of max" rule), worked out from real data during design:** for each cocktail, count how many *distinct* ingredients contributed each flavor tag, find the highest count among those tags, and keep any tag whose count is at least half of that max (`count * 2 >= max`, to avoid integer-division rounding). This was tuned against real examples — an early "must be backed by 2+ ingredients" version was rejected because it failed on cocktails like the Margarita, where a *single* dominant ingredient (Lime juice) carries both Citrus and Sour; the half-of-max rule keeps both since they're tied for the max, while still dropping genuinely minor one-off notes (e.g. the "Ace" cocktail keeps Creamy, backed by 3 ingredients, but drops Herbal/Sweet, each backed by only 1). A cocktail with no tagged ingredients at all (no flavor data to work with) simply gets no `CocktailFlavorTag` rows — verified against real synced data: 407 of 426 cocktails end up with at least one flavor tag.

| Field | Notes |
|---|---|
| CocktailId | FK |
| FlavorTagId | FK |

### IngredientSubstitution
The seeded rule table that powers the substitution engine (Phase 8). Curated data, not user-generated for MVP.

| Field | Notes |
|---|---|
| SourceIngredientId | FK → Ingredient — the ingredient being swapped out |
| ReplacementIngredientId | FK → Ingredient — the suggested alternative |
| Reason | enum: `Taste` / `Cost` / `Supply` — which swap-path this rule applies to |
| Notes | optional freetext, e.g. "slightly sweeter, reduce simple syrup" |

A single source ingredient can have multiple rows here (different replacement per reason).

**Built 2026-09-28 (Phase 8).** Seeded by `IngredientSubstitutionSeedService` from a small curated rule list (13 rules covering all three `Reason` values with real bartending justification, e.g. Cognac→Brandy for Cost, Orgeat→Amaretto for Supply). Rules reference ingredients by name, looked up case-insensitively — the synced ingredient data has inconsistent casing for the same real ingredient (`"Dark Rum"` and `"Dark rum"` both exist as separate rows from Phase 5's sync), so every case variant found gets its own substitution row rather than the rule silently only firing for one casing. Verified: 13 rules expanded to 25 real rows after case-variant matching.

`POST /api/substitutions/suggest` (`SubstitutionsController`) exposes this via the two-question UX flow already sketched in `API_DESIGN.md` before this was built: ask "Taste, or Availability?", then only if Availability, "Cost or Supply?" — mapped onto the stored `Reason` in the controller, kept as a separate wire vocabulary from the enum so the UI flow isn't coupled to the data model's shape.

### CookbookEntry
A saved recipe, private to a Profile. Built 2026-09-28 (Phase 7) — `POST` upserts on `(ProfileId, CocktailId)` (re-saving updates `Notes` rather than duplicating the row or erroring), enforced at the application level the same way every other join table in this project handles uniqueness (check-then-insert), not a DB-level constraint — consistent with the rest of the codebase rather than introducing a new pattern for one table.

| Field | Notes |
|---|---|
| Id | PK |
| ProfileId | FK |
| CocktailId | FK |
| SavedAt | timestamp |
| Notes | nullable freetext — placeholder for future rating/notes phase |

### CookbookEntrySubstitution
Snapshots any substitutions applied *before* saving, so the cookbook entry reflects the adapted recipe, not the original.

| Field | Notes |
|---|---|
| CookbookEntryId | FK |
| OriginalIngredientId | FK → Ingredient |
| SubstitutedIngredientId | FK → Ingredient |

## Taste ranking (`TasteRankingService`, added 2026-09-28)

`GET /api/profiles/{id}/recommendations` scores and sorts every cocktail for a profile. Two-stage design:

**Expanded 2026-10-02** to back `GET /api/cocktails` and `GET /api/cocktails/browse` as well, so match-score ordering applies everywhere cocktails are listed (search/filter results, and the home page's per-category rails), not just the dedicated recommendations page — Brian's call: "we should be using the match score to sort everything." `RankCocktailsForProfileAsync` took on the same search/category/seasons/flavorTags/spirits filters `GetCocktails` already exposed (applied as `.Where()` clauses before scoring, not after — so "top 4" in a category means top 4 *by score*, not top 4 alphabetically re-sorted), and `profileId` became optional: omitted, every cocktail scores `0` and the sort's alphabetical tiebreaker reduces that to plain A-Z order, so the same endpoints work sensibly with or without a profile. `RankedCocktailDto` was retired as a separate type — it was identical to `CocktailSummaryDto` plus `MatchScore`, so `MatchScore` was folded directly into `CocktailSummaryDto` and every cocktail-listing endpoint now returns that one shape. A caller-supplied `profileId` is verified against the authenticated account before use (`404` if it belongs to someone else), the same ownership check used for every other profile-scoped route.

1. **Hard safety exclusion, not a score penalty.** Any cocktail containing an ingredient that matches one of the profile's `ProfileAllergen` entries (via `AllergenHeuristic`) is dropped from the results entirely — allergens are a safety concern, not a taste preference, so a bad match should never just rank lower. Brian's call, 2026-09-28: "we don't want people overlooking an included, potentially deadly, ingredient just because they're a few drinks deep already." Verified against real data: setting `"dairy"` as an allergen drops 88 of 426 cocktails, including `Alexander` and `Pink Lady` (both cream/egg-based), entirely from the results rather than just scoring them low.
2. **Weighted scoring for everything that survives the exclusion.** For each cocktail: every distinct `Spirit` among its ingredients (via `Ingredient.SpiritId`) checked against `ProfileSpiritPreference` (±2), and every `CocktailFlavorTag` checked against `ProfileFlavorPreference` (±1). Spirit preferences outweigh flavor preferences — "likes/dislikes Rum" is a more definitive signal than a flavor nuance like "prefers Citrus." Both weights are plain constants in `TasteRankingService`, easy to retune once real usage exists to judge against. A profile with no preferences set yet gets every cocktail at a neutral `0`, sorted alphabetically as a stable tiebreaker — never an empty or error result. Verified against real data: a profile with "Likes Rum / Dislikes Gin / Prefers Citrus / Avoids Creamy" scores `Alexander` (Gin + Crème de Cacao + Cream) at exactly -3, and rum-and-citrus cocktails at exactly +3.

**Known limitation, documented rather than hidden:** `AllergenHeuristic` matches free-text allergen entries against ingredient names via a small curated category mapping (e.g. `"dairy"` → cream/milk/egg/yogurt keywords, since "dairy" never literally appears in an ingredient name) plus a fallback to direct keyword matching for anything outside that curated list. This is a best-effort match, not a guarantee — an oddly-phrased allergen or an ingredient name that doesn't obviously say what it contains can still be missed. The frontend should carry a visible disclaimer near allergen settings rather than imply a guarantee.

## Design notes worth remembering

- **Why `Sentiment` enums instead of separate Likes/Dislikes tables**: one table with a sentiment column is less schema duplication and makes "show me everything this profile has an opinion on" a single query. Trade-off: slightly less type-safety than separate tables, acceptable here.
- **Why `FlavorTag` is shared between ingredients and profile preferences**: this shared vocabulary is *the* mechanism that makes taste-based ranking possible — we can score "does this cocktail's ingredient flavor tags overlap with what this profile prefers/avoids" directly in SQL/LINQ.
- **Why `IsCustom` lives on `Cocktail` instead of a separate `CustomRecipe` table**: per the brief, we want the data model ready for custom recipes without building the UI yet. Reusing `Cocktail` means custom recipes automatically work with cookbook, substitution, and display logic for free — no parallel code path needed later.
- **`Category` normalization, 2026-10-04**: TheCocktailDB splits plain mixed drinks into `"Ordinary Drink"` separately from `"Cocktail"`, a distinction that doesn't track any consistent rule in their own source data (a Screwdriver and a Daiquiri land on opposite sides of it) and just produced two near-identical rails in the UI. `CocktailDbSyncService.NormalizeCategory` now folds `"Ordinary Drink"` into `"Cocktail"` at sync time, so every future `sync-cocktails` run stays merged automatically; the 192 already-synced rows were updated once directly in the database to match (`UPDATE "Cocktails" SET "Category" = 'Cocktail' WHERE "Category" = 'Ordinary Drink'`).
- **Why `CocktailSeason` is a join table, not a field on `Cocktail`**: decided with Brian in Phase 5 — TheCocktailDB has zero season data, so we assign it ourselves via a heuristic (category/ingredients/serving style). A cocktail can genuinely belong to multiple seasons (a Moscow Mule fits Spring and Summer; a warm spiced drink can fit Fall and Winter), so a single `Season` field would force an inaccurate either/or choice. The heuristic assigns a *set*, not a pick-one value.
