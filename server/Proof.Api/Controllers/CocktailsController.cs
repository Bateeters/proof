using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proof.Api.Data;
using Proof.Api.DTOs;
using Proof.Api.Models;
using Proof.Api.Services;

namespace Proof.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]

public class CocktailsController : ControllerBase
{
    private readonly ProofDbContext _context;
    private readonly WhatCanIMakeService _whatCanIMakeService;
    private readonly TasteRankingService _tasteRankingService;
    private readonly CocktailSimilarityService _similarityService;

    public CocktailsController(
        ProofDbContext context,
        WhatCanIMakeService whatCanIMakeService,
        TasteRankingService tasteRankingService,
        CocktailSimilarityService similarityService)
    {
        _context = context;
        _whatCanIMakeService = whatCanIMakeService;
        _tasteRankingService = tasteRankingService;
        _similarityService = similarityService;
    }

    private Guid GetAccountId()
    {
        var accountIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value;
        return Guid.Parse(accountIdClaim);
    }

    // profileId is client-supplied, so it's verified against the caller's
    // own account before it's trusted for scoring -- otherwise one account
    // could pull another profile's taste/allergen data indirectly through
    // its effect on match scores and exclusions.
    private async Task<bool> IsOwnedByCallerAsync(Guid profileId)
    {
        var accountId = GetAccountId();
        return await _context.Profiles.AnyAsync(p => p.Id == profileId && p.AccountId == accountId);
    }

    [HttpGet]
    public async Task<IActionResult> GetCocktails(
        [FromQuery] string? search,
        [FromQuery] string? category,
        // Comma-separated, same parsing style as WhatCanIMake's ingredients
        // param. Multi-select: a cocktail matches if it has ANY of the
        // selected values within one filter, combined with AND across the
        // three filters — e.g. "Summer or Winter" AND "Sweet or Citrus" AND
        // "Rum or Vodka".
        [FromQuery] string? seasons,
        [FromQuery] string? flavorTags,
        [FromQuery] string? spirits,
        // Optional: when given, results are scored and sorted by match
        // score (highest first, alphabetical tiebreak) for this profile
        // instead of being left in plain alphabetical order.
        [FromQuery] Guid? profileId)
    {
        if (profileId.HasValue && !await IsOwnedByCallerAsync(profileId.Value))
        {
            return NotFound();
        }

        var parsedSeasons = (seasons ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => Enum.TryParse<Season>(s, ignoreCase: true, out var parsed) ? parsed : (Season?)null)
            .Where(s => s.HasValue)
            .Select(s => s!.Value)
            .ToList();

        var parsedFlavorTags = (flavorTags ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        var parsedSpirits = (spirits ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        var cocktails = await _tasteRankingService.RankCocktailsForProfileAsync(
            GetAccountId(), profileId, search, category, parsedSeasons, parsedFlavorTags, parsedSpirits);

        return Ok(cocktails);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCocktailById(Guid id, [FromQuery] Guid? profileId)
    {
        if (profileId.HasValue && !await IsOwnedByCallerAsync(profileId.Value))
        {
            return NotFound();
        }

        var accountId = GetAccountId();
        var savedCocktailIds = await CocktailVisibility.GetSavedCocktailIdsAsync(_context, profileId);

        var cocktail = await _context.Cocktails
            .Where(CocktailVisibility.VisibleTo(accountId, profileId, savedCocktailIds))
            .Include(c => c.CocktailIngredients)
            .ThenInclude(ci => ci.Ingredient)
            .Include(c => c.CocktailFlavorTags)
            .ThenInclude(cft => cft.FlavorTag)
            .Include(c => c.CocktailSeasons)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cocktail == null)
        {
            // Same response whether the id doesn't exist or it exists but
            // is invisible to this caller -- don't leak existence.
            return NotFound();
        }

        var cocktailDetailDto = new CocktailDetailDto
        {
            Id = cocktail.Id,
            Name = cocktail.Name,
            Category = cocktail.Category,
            Glass = cocktail.Glass,
            ImageUrl = cocktail.ImageUrl,
            Instructions = cocktail.Instructions,
            Ingredients = cocktail.CocktailIngredients.Select(ci => new CocktailIngredientDto
            {
                IngredientId = ci.IngredientId,
                IngredientName = ci.Ingredient.Name,
                Measure = ci.Measure
            }).ToList(),
            FlavorTags = cocktail.CocktailFlavorTags.Select(cft => cft.FlavorTag.Name).ToList(),
            FlavorTagIds = cocktail.CocktailFlavorTags.Select(cft => cft.FlavorTagId).ToList(),
            Seasons = cocktail.CocktailSeasons.Select(cs => cs.Season.ToString()).ToList(),
            IsCustom = cocktail.IsCustom,
            Visibility = cocktail.Visibility,
            IsOwnedByCaller = profileId.HasValue && cocktail.OwnerProfileId == profileId.Value
        };

        return Ok(cocktailDetailDto);
    }

    [HttpGet("{id}/similar")]
    public async Task<IActionResult> GetSimilarCocktails(Guid id, [FromQuery] int take = 4, [FromQuery] Guid? profileId = null)
    {
        if (profileId.HasValue && !await IsOwnedByCallerAsync(profileId.Value))
        {
            return NotFound();
        }

        var similar = await _similarityService.GetSimilarCocktailsAsync(GetAccountId(), profileId, id, take);

        if (similar == null)
        {
            return NotFound();
        }

        return Ok(similar);
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories([FromQuery] Guid? profileId)
    {
        if (profileId.HasValue && !await IsOwnedByCallerAsync(profileId.Value))
        {
            return NotFound();
        }

        var savedCocktailIds = await CocktailVisibility.GetSavedCocktailIdsAsync(_context, profileId);
        var categories = await _context.Cocktails
            .Where(CocktailVisibility.VisibleTo(GetAccountId(), profileId, savedCocktailIds))
            .Select(c => c.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("browse")]
    public async Task<IActionResult> Browse([FromQuery] int perCategory = 5, [FromQuery] Guid? profileId = null)
    {
        if (profileId.HasValue && !await IsOwnedByCallerAsync(profileId.Value))
        {
            return NotFound();
        }

        var accountId = GetAccountId();
        var savedCocktailIds = await CocktailVisibility.GetSavedCocktailIdsAsync(_context, profileId);
        var categories = await _context.Cocktails
            .Where(CocktailVisibility.VisibleTo(accountId, profileId, savedCocktailIds))
            .Select(c => c.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        var previews = new List<CategoryPreviewDto>();

        foreach (var category in categories)
        {
            // Ranked per-category first, then truncated -- taking the top
            // N by name and only sorting afterward would show the top 4
            // alphabetically, not the top 4 by match score.
            var ranked = await _tasteRankingService.RankCocktailsForProfileAsync(accountId, profileId, category: category);

            previews.Add(new CategoryPreviewDto { Category = category, Cocktails = ranked.Take(perCategory).ToList() });
        }

        return Ok(previews);
    }

    [HttpGet("what-can-i-make")]
    public async Task<IActionResult> WhatCanIMake([FromQuery] string? ingredients, [FromQuery] Guid? profileId)
    {
        if (profileId.HasValue && !await IsOwnedByCallerAsync(profileId.Value))
        {
            return NotFound();
        }

        var ingredientNames = (ingredients ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);
        var results = await _whatCanIMakeService.FindMakeableCocktailsAsync(GetAccountId(), profileId, ingredientNames);
        return Ok(results);
    }

    // Drives the add/edit cocktail form's live tag/season pre-fill as the
    // user edits the ingredient list -- no ownership check needed (not
    // scoped to any specific cocktail/profile), and lenient on entries it
    // can't resolve (filtered out, never a 400) since this is a live
    // suggestion, not a save-time validation gate.
    [HttpPost("suggest-tags")]
    public async Task<IActionResult> SuggestTags(SuggestTagsRequestDto request)
    {
        var existingIds = request.Ingredients
            .Where(i => i.IngredientId.HasValue)
            .Select(i => i.IngredientId!.Value)
            .ToList();

        var namesById = await _context.Ingredients
            .Where(i => existingIds.Contains(i.Id))
            .ToDictionaryAsync(i => i.Id, i => i.Name);

        var names = new List<string>();
        foreach (var entry in request.Ingredients)
        {
            if (entry.IngredientId.HasValue)
            {
                if (namesById.TryGetValue(entry.IngredientId.Value, out var existingName))
                {
                    names.Add(existingName);
                }
            }
            else if (!string.IsNullOrWhiteSpace(entry.NewIngredientName))
            {
                names.Add(CustomCocktailService.NormalizeIngredientName(entry.NewIngredientName));
            }
        }

        var tagSets = names.Select(n => IngredientFlavorHeuristic.AssignFlavorTag(n));
        var prominentTagNames = CocktailFlavorTagHeuristic.ComputeProminentTags(tagSets);

        var flavorTagIds = await _context.FlavorTags
            .Where(f => prominentTagNames.Contains(f.Name))
            .Select(f => f.Id)
            .ToListAsync();

        var seasons = SeasonHeuristic.AssignSeasons(names).ToList();

        return Ok(new SuggestTagsResponseDto { FlavorTagIds = flavorTagIds, Seasons = seasons });
    }
}