using Microsoft.EntityFrameworkCore;
using Proof.Api.Data;
using Proof.Api.DTOs;
using Proof.Api.Models;

namespace Proof.Api.Services;

/// <summary>
/// Scores and ranks cocktails for a profile based on its spirit/flavor
/// preferences and allergens, optionally narrowed by the same search/
/// category/season/flavor/spirit filters used for browsing. See
/// DATA_MODEL.md "Taste ranking" for the full design writeup.
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

    // profileId is optional so this one method can back every cocktail
    // listing in the app, not just the dedicated recommendations page: with
    // no profile given, every matching cocktail scores 0 and the
    // alphabetical tiebreaker below reduces that to a plain A-Z order. The
    // search/category/seasons/flavorTags/spirits filters are the same ones
    // CocktailsController.GetCocktails exposes — they're applied here too so
    // scoring and filtering share one pass instead of scoring everything
    // and filtering after the fact. callerAccountId is required (every
    // caller is authenticated) and drives CocktailVisibility.VisibleTo, so
    // a custom cocktail invisible to this caller never gets scored/returned
    // in the first place.
    public async Task<List<CocktailSummaryDto>> RankCocktailsForProfileAsync(
        Guid callerAccountId,
        Guid? profileId,
        string? search = null,
        string? category = null,
        List<Season>? seasons = null,
        List<string>? flavorTags = null,
        List<string>? spirits = null)
    {
        var spiritSentimentBySpiritId = new Dictionary<Guid, Sentiment>();
        var flavorSentimentByFlavorTagId = new Dictionary<Guid, Sentiment>();
        var allergens = new List<string>();

        if (profileId.HasValue)
        {
            spiritSentimentBySpiritId = await _context.ProfileSpiritPreferences
                .Where(sp => sp.ProfileId == profileId.Value)
                .ToDictionaryAsync(sp => sp.SpiritId, sp => sp.Sentiment);

            flavorSentimentByFlavorTagId = await _context.ProfileFlavorPreferences
                .Where(fp => fp.ProfileId == profileId.Value)
                .ToDictionaryAsync(fp => fp.FlavorTagId, fp => fp.Sentiment);

            allergens = await _context.ProfileAllergens
                .Where(a => a.ProfileId == profileId.Value)
                .Select(a => a.Name)
                .ToListAsync();
        }

        var savedCocktailIds = await CocktailVisibility.GetSavedCocktailIdsAsync(_context, profileId);
        var query = _context.Cocktails
            .Where(CocktailVisibility.VisibleTo(callerAccountId, profileId, savedCocktailIds));

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Name.ToLower().Contains(search.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(c => c.Category.ToLower() == category.ToLower());
        }

        if (seasons != null && seasons.Count > 0)
        {
            query = query.Where(c => c.CocktailSeasons.Any(cs => seasons.Contains(cs.Season)));
        }

        if (flavorTags != null && flavorTags.Count > 0)
        {
            query = query.Where(c => c.CocktailFlavorTags.Any(cft => flavorTags.Contains(cft.FlavorTag.Name)));
        }

        if (spirits != null && spirits.Count > 0)
        {
            query = query.Where(c => c.CocktailIngredients.Any(ci =>
                ci.Ingredient.SpiritId != null && spirits.Contains(ci.Ingredient.Spirit!.Name)));
        }

        var cocktails = await query
            .Include(c => c.CocktailIngredients).ThenInclude(ci => ci.Ingredient)
            .Include(c => c.CocktailFlavorTags).ThenInclude(cft => cft.FlavorTag)
            .ToListAsync();

        var ranked = new List<CocktailSummaryDto>();

        foreach (var cocktail in cocktails)
        {
            // Hard safety exclusion, not a score penalty — never show a
            // cocktail containing an ingredient that matches a profile's
            // stated allergen, regardless of how well it otherwise scores.
            if (profileId.HasValue)
            {
                var containsAllergen = cocktail.CocktailIngredients.Any(ci =>
                    allergens.Any(allergen => AllergenHeuristic.IngredientMatchesAllergen(ci.Ingredient.Name, allergen)));

                if (containsAllergen)
                {
                    continue;
                }
            }

            var score = 0;

            if (profileId.HasValue)
            {
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
            }

            ranked.Add(new CocktailSummaryDto
            {
                Id = cocktail.Id,
                Name = cocktail.Name,
                Category = cocktail.Category,
                Glass = cocktail.Glass,
                ImageUrl = cocktail.ImageUrl,
                FlavorTags = cocktail.CocktailFlavorTags.Select(cft => cft.FlavorTag.Name).ToList(),
                MatchScore = score
            });
        }

        // Highest score first; alphabetical is just a stable, predictable
        // tiebreaker for equally-scored cocktails (e.g. everything at 0
        // when no profile context is given), not a meaningful signal.
        return ranked
            .OrderByDescending(r => r.MatchScore)
            .ThenBy(r => r.Name)
            .ToList();
    }
}
