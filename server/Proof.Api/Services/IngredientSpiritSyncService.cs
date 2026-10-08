using Microsoft.EntityFrameworkCore;
using Proof.Api.Data;

namespace Proof.Api.Services;

public class IngredientSpiritSyncService
{
    private readonly ProofDbContext _context;

    public IngredientSpiritSyncService(ProofDbContext context)
    {
        _context = context;
    }

    public async Task<int> IdentifyAllIngredientSpiritsAsync()
    {
        var spiritIdsByName = await _context.Spirits
            .ToDictionaryAsync(s => s.Name, s => s.Id);

        // IsManuallyTagged ingredients are excluded -- a user explicitly
        // picked this ingredient's spirit by hand (custom-cocktail
        // ingredient creation, when the heuristic found no match), and a
        // bulk re-run shouldn't silently overwrite that correction.
        var ingredients = await _context.Ingredients.Where(i => !i.IsManuallyTagged).ToListAsync();

        var ingredientsMatched = 0;

        foreach (var ingredient in ingredients)
        {
            var spiritName = IngredientSpiritHeuristic.IdentifySpirit(ingredient.Name);

            // Setting this every run (not just when a match is found) means a
            // keyword-list change that removes a previously-wrong match
            // actually clears it, rather than leaving a stale SpiritId behind.
            ingredient.SpiritId = spiritName != null && spiritIdsByName.TryGetValue(spiritName, out var spiritId)
                ? spiritId
                : null;

            if (ingredient.SpiritId != null)
            {
                ingredientsMatched++;
            }
        }

        await _context.SaveChangesAsync();
        return ingredientsMatched;
    }
}
