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
        // TODO:
        // 1. Load everything you'll need into memory up front, so the per-cocktail
        //    work below doesn't have to hit the database again and again:
        //    - Every Cocktail (or just their Ids).
        //    - Every CocktailIngredient (tells you which Ingredients belong to which Cocktail).
        //    - Every IngredientFlavorTag (tells you which FlavorTags belong to which Ingredient).
        //
        // 2. Same as the ingredient sync: wipe any CocktailFlavorTags left over from a
        //    previous run before rebuilding (we already solved this pattern once).
        //
        // 3. For each cocktail:
        //    a. Find which Ingredients belong to it (from the CocktailIngredients you loaded).
        //    b. For those ingredients, find every FlavorTag they carry (from the
        //       IngredientFlavorTags you loaded).
        //    c. Group by FlavorTagId, and for each group, count how many *distinct*
        //       ingredients contributed it — this is the number from the Ace/Margarita/A1
        //       examples (e.g. Creamy came from 3 different ingredients in Ace).
        //    d. Find the highest count among those groups for this cocktail (the "max").
        //    e. Keep any FlavorTagId whose count is at least half of the max. Careful with
        //       integer math here — "count >= max / 2" using plain integer division can
        //       round in a way you don't want. Think about how to compare the two without
        //       dividing at all.
        //    f. Add a CocktailFlavorTag row for each FlavorTagId that qualifies.
        //
        // 4. Save, and return a count of how many rows got added.

        throw new NotImplementedException();
    }
}
