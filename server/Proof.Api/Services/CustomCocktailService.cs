using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Proof.Api.Data;
using Proof.Api.DTOs;
using Proof.Api.Models;

namespace Proof.Api.Services;

/// <summary>
/// Create/update/delete for profile-authored cocktails. See
/// DATA_MODEL.md "Custom cocktails" for the full design writeup.
/// </summary>
public class CustomCocktailService
{
    private readonly ProofDbContext _context;

    public CustomCocktailService(ProofDbContext context)
    {
        _context = context;
    }

    // Case-insensitive match-or-create against the shared Ingredient
    // catalog. No IngredientFlavorTag creation here -- flavor tags are
    // always user-confirmed at the cocktail level for custom cocktails
    // (CocktailsController.SuggestTags / SaveCustomCocktailDto.FlavorTagIds),
    // so there's nothing to derive from a per-ingredient tag that only ever
    // existed to support that derivation for the original TheCocktailDB
    // import (see CocktailFlavorTagSyncService).
    public async Task<(bool Success, string? Error, Ingredient? Ingredient)> ResolveIngredientAsync(CocktailIngredientInputDto input)
    {
        var hasId = input.IngredientId.HasValue;
        var hasName = !string.IsNullOrWhiteSpace(input.NewIngredientName);

        if (hasId == hasName)
        {
            return (false, "Each ingredient must specify exactly one of an existing ingredient or a new ingredient name.", null);
        }

        if (hasId)
        {
            var existing = await _context.Ingredients.FindAsync(input.IngredientId!.Value);
            return existing == null ? (false, "Unknown ingredient.", null) : (true, null, existing);
        }

        var normalized = NormalizeIngredientName(input.NewIngredientName!);
        if (normalized.Length == 0)
        {
            return (false, "Ingredient name cannot be blank.", null);
        }

        var caseInsensitiveMatch = await _context.Ingredients
            .FirstOrDefaultAsync(i => i.Name.ToLower() == normalized.ToLower());
        if (caseInsensitiveMatch != null)
        {
            // Reusing an already-resolved ingredient -- any SpiritId the
            // request supplied for a "new" ingredient doesn't apply here.
            return (true, null, caseInsensitiveMatch);
        }

        var ingredient = new Ingredient
        {
            Name = normalized,
            Type = IngredientType.Other,
            CostTier = CostTier.Mid,
            AvailabilityTier = AvailabilityTier.Common
        };

        if (input.SpiritId.HasValue)
        {
            var manualSpirit = await _context.Spirits.FindAsync(input.SpiritId.Value);
            if (manualSpirit == null)
            {
                return (false, "Unknown spirit.", null);
            }

            ingredient.SpiritId = manualSpirit.Id;
            ingredient.IsManuallyTagged = true;
        }
        else
        {
            var spiritName = IngredientSpiritHeuristic.IdentifySpirit(normalized);
            if (spiritName != null)
            {
                // Skip rather than throw if the heuristic's name doesn't
                // match the seeded Spirits table -- defensive against the
                // two ever drifting apart.
                var spirit = await _context.Spirits.FirstOrDefaultAsync(s => s.Name == spiritName);
                if (spirit != null)
                {
                    ingredient.SpiritId = spirit.Id;
                }
            }
        }

        _context.Ingredients.Add(ingredient);
        return (true, null, ingredient);
    }

    // Public so CocktailsController.SuggestTags can resolve a not-yet-saved
    // free-text ingredient name the same way a real save would, keeping the
    // live suggestion endpoint's name resolution identical to the actual
    // save path.
    public static string NormalizeIngredientName(string raw)
    {
        var collapsed = Regex.Replace(raw.Trim(), @"\s+", " ");
        return collapsed.Length == 0 ? collapsed : CultureInfo.InvariantCulture.TextInfo.ToTitleCase(collapsed.ToLower());
    }

    // Category is a free string, not a lookup-table entity like Ingredient
    // -- just trim/collapse whitespace, no forced Title Case (unlike
    // ingredients), since real existing category values like "IBA Cocktail"
    // would get mangled by it.
    private static string NormalizeCategory(string raw) => Regex.Replace(raw.Trim(), @"\s+", " ");

    public async Task<(bool Success, string? Error, Cocktail? Cocktail)> CreateAsync(Guid ownerProfileId, SaveCustomCocktailDto request)
    {
        var validationError = ValidateFields(request);
        if (validationError != null)
        {
            return (false, validationError, null);
        }

        var cocktail = new Cocktail
        {
            Name = request.Name.Trim(),
            Category = NormalizeCategory(request.Category),
            Glass = request.Glass.Trim(),
            Instructions = request.Instructions.Trim(),
            ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim(),
            IsCustom = true,
            OwnerProfileId = ownerProfileId,
            Visibility = request.Visibility
        };
        _context.Cocktails.Add(cocktail);

        var applyError = await ApplyIngredientsAndTagsAsync(cocktail, request);
        if (applyError != null)
        {
            return (false, applyError, null);
        }

        await _context.SaveChangesAsync();
        return (true, null, cocktail);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(Cocktail existing, SaveCustomCocktailDto request)
    {
        var validationError = ValidateFields(request);
        if (validationError != null)
        {
            return (false, validationError);
        }

        existing.Name = request.Name.Trim();
        existing.Category = NormalizeCategory(request.Category);
        existing.Glass = request.Glass.Trim();
        existing.Instructions = request.Instructions.Trim();
        existing.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim();
        existing.Visibility = request.Visibility;

        // Full replace, not a diff -- same pattern
        // ProfilesController.UpdatePreferences already uses for its own
        // child rows.
        var existingIngredients = await _context.CocktailIngredients.Where(ci => ci.CocktailId == existing.Id).ToListAsync();
        _context.CocktailIngredients.RemoveRange(existingIngredients);
        var existingFlavorTags = await _context.CocktailFlavorTags.Where(cft => cft.CocktailId == existing.Id).ToListAsync();
        _context.CocktailFlavorTags.RemoveRange(existingFlavorTags);
        var existingSeasons = await _context.CocktailSeasons.Where(cs => cs.CocktailId == existing.Id).ToListAsync();
        _context.CocktailSeasons.RemoveRange(existingSeasons);

        var applyError = await ApplyIngredientsAndTagsAsync(existing, request);
        if (applyError != null)
        {
            return (false, applyError);
        }

        await _context.SaveChangesAsync();
        return (true, null);
    }

    private static string? ValidateFields(SaveCustomCocktailDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return "Name is required.";
        if (string.IsNullOrWhiteSpace(request.Category)) return "Category is required.";
        if (string.IsNullOrWhiteSpace(request.Glass)) return "Glass is required.";
        if (string.IsNullOrWhiteSpace(request.Instructions)) return "Instructions are required.";
        if (request.Ingredients.Count == 0) return "At least one ingredient is required.";
        return null;
    }

    // Shared by Create and Update -- resolves/attaches ingredients, flavor
    // tags, and seasons onto an already-tracked (new or existing) cocktail.
    // Nothing is persisted here; the caller's single SaveChangesAsync at
    // the end is what actually commits, so a validation failure partway
    // through leaves nothing partially written.
    private async Task<string?> ApplyIngredientsAndTagsAsync(Cocktail cocktail, SaveCustomCocktailDto request)
    {
        var sortOrder = 0;
        foreach (var ingredientInput in request.Ingredients)
        {
            var (success, error, ingredient) = await ResolveIngredientAsync(ingredientInput);
            if (!success)
            {
                return error;
            }

            _context.CocktailIngredients.Add(new CocktailIngredient
            {
                Cocktail = cocktail,
                CocktailId = cocktail.Id,
                Ingredient = ingredient!,
                IngredientId = ingredient!.Id,
                Measure = ingredientInput.Measure,
                SortOrder = sortOrder++
            });
        }

        foreach (var flavorTagId in request.FlavorTagIds)
        {
            var exists = await _context.FlavorTags.AnyAsync(f => f.Id == flavorTagId);
            if (!exists)
            {
                return "Unknown flavor tag.";
            }

            _context.CocktailFlavorTags.Add(new CocktailFlavorTag { Cocktail = cocktail, CocktailId = cocktail.Id, FlavorTagId = flavorTagId });
        }

        foreach (var season in request.Seasons)
        {
            _context.CocktailSeasons.Add(new CocktailSeason { Cocktail = cocktail, CocktailId = cocktail.Id, Season = season });
        }

        return null;
    }
}
