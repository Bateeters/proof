using Microsoft.EntityFrameworkCore;
using Proof.Api.Data;
using Proof.Api.Models;

namespace Proof.Api.Services;

public class CocktailFlavorTagSyncService
{
    private readonly ProofDbContext _context;

    public CocktailFlavorTagSyncService(ProofDbContext context)
    {
        _context = context;
    }

    public async Task<int> TagAllCocktailsAsync()
    {
        // Custom cocktails are excluded -- their flavor tags are always
        // user-confirmed (CocktailsController.SuggestTags pre-fills a
        // multi-select the user can freely adjust before saving), so a
        // wholesale re-tag here would silently overwrite that choice.
        var cocktailIds = await _context.Cocktails
            .Where(c => !c.IsCustom)
            .Select(c => c.Id)
            .ToListAsync();

        var cocktailIngredients = await _context.CocktailIngredients
            .Select(ci => new { ci.CocktailId, ci.IngredientId })
            .ToListAsync();

        var ingredientFlavorTags = await _context.IngredientFlavorTags
            .Select(ift => new { ift.IngredientId, ift.FlavorTagId })
            .ToListAsync();

        // Scoped to the same non-custom cocktails -- a plain RemoveRange of
        // every CocktailFlavorTag would wipe custom cocktails' tags here
        // without the loop below ever re-adding them (it only iterates
        // cocktailIds, which already excludes IsCustom).
        var existingCocktailTags = await _context.CocktailFlavorTags
            .Where(cft => cocktailIds.Contains(cft.CocktailId))
            .ToListAsync();
        _context.CocktailFlavorTags.RemoveRange(existingCocktailTags);

        var ingredientIdsByCocktail = cocktailIngredients
            .GroupBy(ci => ci.CocktailId)
            .ToDictionary(g => g.Key, g => g.Select(ci => ci.IngredientId).ToList());

        var flavorTagIdsByIngredient = ingredientFlavorTags
            .GroupBy(ift => ift.IngredientId)
            .ToDictionary(g => g.Key, g => g.Select(ift => ift.FlavorTagId).ToList());

        var cocktailFlavorTagsAdded = 0;

        foreach (var cocktailId in cocktailIds)
        {
            if (!ingredientIdsByCocktail.TryGetValue(cocktailId, out var ingredientIds))
            {
                continue;
            }

            // Every (Ingredient, FlavorTag) pair contributed by this cocktail's
            // ingredients. Kept as pairs (not just a flat list of tag ids) so the
            // grouping below can count *distinct ingredients* per tag rather than
            // raw occurrences.
            var pairs = new List<(Guid IngredientId, Guid FlavorTagId)>();
            foreach (var ingredientId in ingredientIds)
            {
                if (flavorTagIdsByIngredient.TryGetValue(ingredientId, out var tagIds))
                {
                    foreach (var tagId in tagIds)
                    {
                        pairs.Add((ingredientId, tagId));
                    }
                }
            }

            if (pairs.Count == 0)
            {
                // No ingredient in this cocktail has any flavor data at all —
                // nothing to compute a profile from, so it stays untagged.
                continue;
            }

            var ingredientCountByTag = pairs
                .GroupBy(p => p.FlavorTagId)
                .Select(g => new
                {
                    FlavorTagId = g.Key,
                    IngredientCount = g.Select(p => p.IngredientId).Distinct().Count()
                })
                .ToList();

            var maxIngredientCount = ingredientCountByTag.Max(t => t.IngredientCount);

            // "At least half of the max" — e.g. IngredientCount * 2 >= maxIngredientCount
            // avoids integer-division rounding. A tag tied for the max (or close to it)
            // is prominent; one backed by a single minor ingredient next to a dominant
            // tag gets dropped.
            foreach (var tag in ingredientCountByTag)
            {
                if (tag.IngredientCount * 2 >= maxIngredientCount)
                {
                    _context.CocktailFlavorTags.Add(new CocktailFlavorTag
                    {
                        CocktailId = cocktailId,
                        FlavorTagId = tag.FlavorTagId
                    });
                    cocktailFlavorTagsAdded++;
                }
            }
        }

        await _context.SaveChangesAsync();
        return cocktailFlavorTagsAdded;
    }
}
