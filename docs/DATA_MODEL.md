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
| Category | e.g. "Ordinary Drink", "Cocktail", "Punch" (from source data) |
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

### IngredientSubstitution
The seeded rule table that powers the substitution engine (Phase 8). Curated data, not user-generated for MVP.

| Field | Notes |
|---|---|
| SourceIngredientId | FK → Ingredient — the ingredient being swapped out |
| ReplacementIngredientId | FK → Ingredient — the suggested alternative |
| Reason | enum: `Taste` / `Cost` / `Supply` — which swap-path this rule applies to |
| Notes | optional freetext, e.g. "slightly sweeter, reduce simple syrup" |

A single source ingredient can have multiple rows here (different replacement per reason).

### CookbookEntry
A saved recipe, private to a Profile.

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

## Design notes worth remembering

- **Why `Sentiment` enums instead of separate Likes/Dislikes tables**: one table with a sentiment column is less schema duplication and makes "show me everything this profile has an opinion on" a single query. Trade-off: slightly less type-safety than separate tables, acceptable here.
- **Why `FlavorTag` is shared between ingredients and profile preferences**: this shared vocabulary is *the* mechanism that makes taste-based ranking possible — we can score "does this cocktail's ingredient flavor tags overlap with what this profile prefers/avoids" directly in SQL/LINQ.
- **Why `IsCustom` lives on `Cocktail` instead of a separate `CustomRecipe` table**: per the brief, we want the data model ready for custom recipes without building the UI yet. Reusing `Cocktail` means custom recipes automatically work with cookbook, substitution, and display logic for free — no parallel code path needed later.
