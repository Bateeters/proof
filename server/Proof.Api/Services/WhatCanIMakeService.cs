using Microsoft.EntityFrameworkCore;
using Proof.Api.Data;
using Proof.Api.DTOs;

namespace Proof.Api.Services;

public class WhatCanIMakeService
{
    private readonly ProofDbContext _context;

    public WhatCanIMakeService(ProofDbContext context)
    {
        _context = context;
    }

    public async Task<List<WhatCanIMakeResultDto>> FindMakeableCocktailsAsync(IEnumerable<string> haveIngredientNames)
    {
        // Case-insensitive exact-name matching, not fuzzy/substring like the
        // flavor heuristics — "what can I make" is an inventory check, and a
        // fuzzy match here could tell someone they can make a drink when
        // they don't actually have the right ingredient. Case-insensitive
        // still matters: the synced data has inconsistent casing for the
        // same real ingredient (e.g. "Dark Rum" vs "Dark rum").
        var haveSet = haveIngredientNames
            .Select(name => name.Trim().ToLower())
            .Where(name => name.Length > 0)
            .ToHashSet();

        if (haveSet.Count == 0)
        {
            return [];
        }

        var cocktails = await _context.Cocktails
            .Include(c => c.CocktailIngredients).ThenInclude(ci => ci.Ingredient)
            .ToListAsync();

        var results = new List<WhatCanIMakeResultDto>();

        foreach (var cocktail in cocktails)
        {
            var missingIngredients = cocktail.CocktailIngredients
                .Select(ci => ci.Ingredient.Name)
                .Where(name => !haveSet.Contains(name.ToLower()))
                .ToList();

            var matchedCount = cocktail.CocktailIngredients.Count - missingIngredients.Count;

            // Skip cocktails with zero overlap -- otherwise every one of the
            // 426 cocktails would come back, most of them completely
            // irrelevant to what was actually listed.
            if (matchedCount == 0)
            {
                continue;
            }

            results.Add(new WhatCanIMakeResultDto
            {
                Id = cocktail.Id,
                Name = cocktail.Name,
                Category = cocktail.Category,
                Glass = cocktail.Glass,
                ImageUrl = cocktail.ImageUrl,
                MissingIngredients = missingIngredients
            });
        }

        return results
            .OrderBy(r => r.MissingIngredients.Count)
            .ThenBy(r => r.Name)
            .ToList();
    }
}
