using Microsoft.EntityFrameworkCore;
using Proof.Api.Data;
using Proof.Api.DTOs;

namespace Proof.Api.Services;

/// <summary>
/// Finds cocktails similar to a given one for the detail page's "More Like
/// This" rail. Content-based, not profile-personalized -- "Recommended For
/// You" already covers taste-personalized ranking (see
/// TasteRankingService); this is the classic "related items" pattern,
/// scored purely from how much two cocktails have in common.
/// </summary>
public class CocktailSimilarityService
{
    // Flavor overlap is the strongest taste signal (same reasoning as
    // TasteRankingService's weighting), spirit overlap close behind since
    // sharing a base spirit is a strong "you'd reach for these together"
    // signal, category match a smaller bonus since it's a coarser bucket
    // (e.g. "Shot" covers a lot of ground). Plain constants, easy to retune.
    private const int FlavorTagOverlapWeight = 2;
    private const int SpiritOverlapWeight = 2;
    private const int CategoryMatchBonus = 1;

    private readonly ProofDbContext _context;

    public CocktailSimilarityService(ProofDbContext context)
    {
        _context = context;
    }

    public async Task<List<CocktailSummaryDto>?> GetSimilarCocktailsAsync(Guid cocktailId, int take = 4)
    {
        var target = await _context.Cocktails
            .Include(c => c.CocktailIngredients).ThenInclude(ci => ci.Ingredient)
            .Include(c => c.CocktailFlavorTags).ThenInclude(cft => cft.FlavorTag)
            .FirstOrDefaultAsync(c => c.Id == cocktailId);

        if (target == null)
        {
            return null;
        }

        var targetFlavorTagIds = target.CocktailFlavorTags.Select(cft => cft.FlavorTagId).ToHashSet();
        var targetSpiritIds = target.CocktailIngredients
            .Select(ci => ci.Ingredient.SpiritId)
            .Where(spiritId => spiritId.HasValue)
            .Select(spiritId => spiritId!.Value)
            .ToHashSet();

        // Narrow to candidates that share at least one signal before
        // materializing/scoring in memory -- same filter-then-score shape
        // TasteRankingService uses, rather than pulling all 426 cocktails
        // in just to score most of them 0.
        var candidates = await _context.Cocktails
            .Where(c => c.Id != cocktailId && (
                c.Category == target.Category ||
                c.CocktailFlavorTags.Any(cft => targetFlavorTagIds.Contains(cft.FlavorTagId)) ||
                c.CocktailIngredients.Any(ci => ci.Ingredient.SpiritId != null && targetSpiritIds.Contains(ci.Ingredient.SpiritId!.Value))))
            .Include(c => c.CocktailIngredients).ThenInclude(ci => ci.Ingredient)
            .Include(c => c.CocktailFlavorTags).ThenInclude(cft => cft.FlavorTag)
            .ToListAsync();

        var scored = candidates.Select(c =>
        {
            var sharedFlavorTags = c.CocktailFlavorTags.Count(cft => targetFlavorTagIds.Contains(cft.FlavorTagId));

            var candidateSpiritIds = c.CocktailIngredients
                .Select(ci => ci.Ingredient.SpiritId)
                .Where(spiritId => spiritId.HasValue)
                .Select(spiritId => spiritId!.Value)
                .ToHashSet();
            var sharedSpirits = candidateSpiritIds.Count(targetSpiritIds.Contains);

            var score = sharedFlavorTags * FlavorTagOverlapWeight
                + sharedSpirits * SpiritOverlapWeight
                + (c.Category == target.Category ? CategoryMatchBonus : 0);

            return new CocktailSummaryDto
            {
                Id = c.Id,
                Name = c.Name,
                Category = c.Category,
                Glass = c.Glass,
                ImageUrl = c.ImageUrl,
                FlavorTags = c.CocktailFlavorTags.Select(cft => cft.FlavorTag.Name).ToList(),
                MatchScore = score
            };
        });

        return scored
            .OrderByDescending(c => c.MatchScore)
            .ThenBy(c => c.Name)
            .Take(take)
            .ToList();
    }
}
