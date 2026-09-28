using Microsoft.EntityFrameworkCore;
using Proof.Api.Data;
using Proof.Api.Models;

namespace Proof.Api.Services;

/// <summary>
/// Seeds the curated IngredientSubstitution rule table (Phase 8). Rules
/// reference real Ingredient rows by name, looked up case-insensitively
/// since the synced ingredient data has inconsistent casing for the same
/// real-world ingredient (e.g. "Dark Rum" and "Dark rum" both exist as
/// separate rows) — every case variant found gets its own substitution row
/// so the rule actually fires regardless of which variant a given cocktail
/// happens to use.
/// </summary>
public class IngredientSubstitutionSeedService
{
    private readonly ProofDbContext _context;

    private static readonly (string Source, string Replacement, SubstitutionReason Reason, string Notes)[] Rules =
    {
        ("Gin", "Vodka", SubstitutionReason.Taste,
            "Swaps Gin's herbal/botanical character for a neutral spirit — changes the drink's core flavor, not just a stand-in."),
        ("Bourbon", "Rye Whiskey", SubstitutionReason.Taste,
            "Rye brings a spicier, drier character than Bourbon's sweeter, corn-forward profile."),
        ("Dark Rum", "Spiced rum", SubstitutionReason.Taste,
            "Adds warm baking-spice notes on top of the base rum character."),
        ("Sweet Vermouth", "Dry Vermouth", SubstitutionReason.Taste,
            "Shifts a drink from sweet/rich toward dry/crisp."),

        ("Cognac", "Brandy", SubstitutionReason.Cost,
            "Cognac is a specific (pricier) type of brandy from the Cognac region — a general brandy is a cheaper stand-in with a similar character."),
        ("Grand Marnier", "Triple Sec", SubstitutionReason.Cost,
            "Grand Marnier is a premium cognac-based orange liqueur; Triple Sec gives similar orange character for a fraction of the price."),
        ("Cointreau", "Triple Sec", SubstitutionReason.Cost,
            "Cointreau is a premium triple sec; a standard triple sec is a budget-friendly stand-in."),
        ("Champagne", "Prosecco", SubstitutionReason.Cost,
            "Prosecco is a noticeably cheaper sparkling wine with a similar role in a cocktail."),

        ("Orgeat syrup", "Amaretto", SubstitutionReason.Supply,
            "Orgeat (almond syrup) isn't always on hand — Amaretto brings a similar almond note, though it adds alcohol and sweetness orgeat doesn't."),
        ("Creme de Cassis", "Grenadine", SubstitutionReason.Supply,
            "Both are sweet and deep red — grenadine is far more commonly stocked when blackcurrant liqueur isn't available."),
        ("Angostura Bitters", "Bitters", SubstitutionReason.Supply,
            "A specific aromatic bitters brand isn't always on hand — a general bitters bottle serves the same balancing role in most recipes."),
        ("Half-and-half", "Milk", SubstitutionReason.Supply,
            "Half-and-half isn't always stocked — milk is the more commonly available substitute, though it's lighter-bodied."),
        ("Lemon Juice", "Lime Juice", SubstitutionReason.Supply,
            "Both are tart, fresh citrus — a reasonable stand-in when one isn't on hand, though the flavor shifts slightly.")
    };

    public IngredientSubstitutionSeedService(ProofDbContext context)
    {
        _context = context;
    }

    public async Task<int> SeedSubstitutionsAsync()
    {
        var ingredients = await _context.Ingredients
            .Select(i => new { i.Id, i.Name })
            .ToListAsync();

        var ingredientIdsByLowerName = ingredients
            .GroupBy(i => i.Name.ToLower())
            .ToDictionary(g => g.Key, g => g.Select(i => i.Id).ToList());

        var existingRules = await _context.IngredientSubstitutions
            .Select(s => new { s.SourceIngredientId, s.ReplacementIngredientId, s.Reason })
            .ToListAsync();
        var existingRuleKeys = existingRules
            .Select(s => (s.SourceIngredientId, s.ReplacementIngredientId, s.Reason))
            .ToHashSet();

        var rulesAdded = 0;

        foreach (var (sourceName, replacementName, reason, notes) in Rules)
        {
            if (!ingredientIdsByLowerName.TryGetValue(sourceName.ToLower(), out var sourceIds))
            {
                continue;
            }

            if (!ingredientIdsByLowerName.TryGetValue(replacementName.ToLower(), out var replacementIds))
            {
                continue;
            }

            foreach (var sourceId in sourceIds)
            {
                foreach (var replacementId in replacementIds)
                {
                    var key = (sourceId, replacementId, reason);
                    if (existingRuleKeys.Contains(key))
                    {
                        continue;
                    }

                    _context.IngredientSubstitutions.Add(new IngredientSubstitution
                    {
                        SourceIngredientId = sourceId,
                        ReplacementIngredientId = replacementId,
                        Reason = reason,
                        Notes = notes
                    });

                    existingRuleKeys.Add(key);
                    rulesAdded++;
                }
            }
        }

        await _context.SaveChangesAsync();
        return rulesAdded;
    }
}
