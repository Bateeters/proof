using Microsoft.EntityFrameworkCore;
using Proof.Api.Data;
using Proof.Api.Models;

namespace Proof.Api.Services;

public class IngredientFlavorTagSyncService
{
    private readonly ProofDbContext _context;

    public IngredientFlavorTagSyncService(ProofDbContext context)
    {
        _context = context;
    }

    public async Task<int> TagAllIngredientsAsync()
    {
        // TODO:
        // 1. Load all FlavorTags from the DB into a lookup you can go from
        //    tag NAME (what the heuristic returns) -> FlavorTag ID (what
        //    IngredientFlavorTag actually needs to store).
        // 2. Load all Ingredients.
        // 3. Decide how to handle ingredients that already have flavor tags
        //    from a previous run (this heuristic's keyword lists might change
        //    later, so re-running it should produce correct results, not just
        //    pile up alongside stale ones). We already solved a version of
        //    this exact problem somewhere else in this codebase — worth a look.
        // 4. For each ingredient, call IngredientFlavorHeuristic.AssignFlavorTag,
        //    map each returned tag name to its FlavorTag ID via your lookup,
        //    and add an IngredientFlavorTag row for each one.
        // 5. Save, and return some kind of count so the endpoint can report
        //    back what happened.

        throw new NotImplementedException();
    }
}
