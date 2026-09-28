using Microsoft.EntityFrameworkCore;
using Proof.Api.Data;
using Proof.Api.DTOs;
using Proof.Api.Models;

namespace Proof.Api.Services;

/// <summary>
/// Scores and ranks cocktails for a profile based on its spirit/flavor
/// preferences and allergens. See DATA_MODEL.md "Taste ranking" for the
/// full design writeup.
/// </summary>
public class TasteRankingService
{
    // Spirit preferences are weighted higher than flavor preferences — "I
    // like/dislike Rum" is a more definitive, dealbreaker-ish signal than a
    // flavor nuance like "prefers Citrus". Plain constants, easy to retune
    // once real usage shows whether ranking feels right.
    private const int SpiritWeight = 2;
    private const int FlavorWeight = 1;

    private readonly ProofDbContext _context;

    public TasteRankingService(ProofDbContext context)
    {
        _context = context;
    }

    public async Task<List<RankedCocktailDto>> RankCocktailsForProfileAsync(Guid profileId)
    {
        var spiritSentimentBySpiritId = await _context.ProfileSpiritPreferences
            .Where(sp => sp.ProfileId == profileId)
            .ToDictionaryAsync(sp => sp.SpiritId, sp => sp.Sentiment);

        var flavorSentimentByFlavorTagId = await _context.ProfileFlavorPreferences
            .Where(fp => fp.ProfileId == profileId)
            .ToDictionaryAsync(fp => fp.FlavorTagId, fp => fp.Sentiment);

        var allergens = await _context.ProfileAllergens
            .Where(a => a.ProfileId == profileId)
            .Select(a => a.Name)
            .ToListAsync();

        var cocktails = await _context.Cocktails
            .Include(c => c.CocktailIngredients).ThenInclude(ci => ci.Ingredient)
            .Include(c => c.CocktailFlavorTags)
            .ToListAsync();

        var ranked = new List<RankedCocktailDto>();

        foreach (var cocktail in cocktails)
        {
            // Hard safety exclusion, not a score penalty — never show a
            // cocktail containing an ingredient that matches a profile's
            // stated allergen, regardless of how well it otherwise scores.
            var containsAllergen = cocktail.CocktailIngredients.Any(ci =>
                allergens.Any(allergen => AllergenHeuristic.IngredientMatchesAllergen(ci.Ingredient.Name, allergen)));

            if (containsAllergen)
            {
                continue;
            }

            var score = 0;

            var spiritIdsInCocktail = cocktail.CocktailIngredients
                .Select(ci => ci.Ingredient.SpiritId)
                .Where(spiritId => spiritId.HasValue)
                .Select(spiritId => spiritId!.Value)
                .Distinct();

            foreach (var spiritId in spiritIdsInCocktail)
            {
                if (spiritSentimentBySpiritId.TryGetValue(spiritId, out var sentiment))
                {
                    score += sentiment == Sentiment.Positive ? SpiritWeight : -SpiritWeight;
                }
            }

            foreach (var cocktailFlavorTag in cocktail.CocktailFlavorTags)
            {
                if (flavorSentimentByFlavorTagId.TryGetValue(cocktailFlavorTag.FlavorTagId, out var sentiment))
                {
                    score += sentiment == Sentiment.Positive ? FlavorWeight : -FlavorWeight;
                }
            }

            ranked.Add(new RankedCocktailDto
            {
                Id = cocktail.Id,
                Name = cocktail.Name,
                Category = cocktail.Category,
                Glass = cocktail.Glass,
                ImageUrl = cocktail.ImageUrl,
                MatchScore = score
            });
        }

        // Highest score first; alphabetical is just a stable, predictable
        // tiebreaker for equally-scored cocktails (e.g. everything at 0
        // when a profile has no preferences set yet), not a meaningful signal.
        return ranked
            .OrderByDescending(r => r.MatchScore)
            .ThenBy(r => r.Name)
            .ToList();
    }
}
