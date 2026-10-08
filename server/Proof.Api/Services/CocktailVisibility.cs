using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Proof.Api.Data;
using Proof.Api.Models;

namespace Proof.Api.Services;

/// <summary>
/// The single predicate gating every read of Cocktails once custom
/// cocktails exist. See DATA_MODEL.md "Custom cocktails" for the full
/// design writeup.
/// </summary>
public static class CocktailVisibility
{
    // Every VisibleTo call site needs this first -- centralized here so
    // the same query isn't hand-copied at each of the 7 read call sites.
    public static async Task<HashSet<Guid>> GetSavedCocktailIdsAsync(ProofDbContext context, Guid? callerProfileId)
    {
        if (!callerProfileId.HasValue)
        {
            return [];
        }

        return await context.CookbookEntries
            .Where(ce => ce.ProfileId == callerProfileId.Value)
            .Select(ce => ce.CocktailId)
            .ToHashSetAsync();
    }

    // EF-translatable: OwnerProfile!.AccountId becomes a LEFT JOIN (never a
    // client-side null-deref -- EF builds SQL from the expression tree, it
    // doesn't evaluate it in-process), and HashSet.Contains becomes
    // SQL IN (...), the same pattern already used for the season/flavor/
    // spirit multi-select filters elsewhere in CocktailsController.
    //
    // alreadySavedCocktailIds (the caller's own CookbookEntry.CocktailId
    // values) does double duty: it's what lets a profile keep access to a
    // cocktail it already saved even after the owner deletes it or narrows
    // its visibility, and because it's scoped to the caller's own cookbook
    // -- not a global check -- a stranger who never saved it still gets a
    // 404 even if they guess the id.
    public static Expression<Func<Cocktail, bool>> VisibleTo(
        Guid callerAccountId, Guid? callerProfileId, HashSet<Guid> alreadySavedCocktailIds) =>
        c => (!c.IsDeleted || alreadySavedCocktailIds.Contains(c.Id))
            && (
                !c.IsCustom
                || c.Visibility == Visibility.Global
                || (c.Visibility == Visibility.Local && c.OwnerProfile!.AccountId == callerAccountId)
                || (c.Visibility == Visibility.Private && c.OwnerProfileId == callerProfileId)
                || alreadySavedCocktailIds.Contains(c.Id)
            );
}
