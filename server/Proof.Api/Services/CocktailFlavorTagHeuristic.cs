namespace Proof.Api.Services;

/// <summary>
/// The "half of max prominence" math extracted from
/// CocktailFlavorTagSyncService into a reusable pure function over tag-
/// name sets, so it works for not-yet-persisted ingredients too (the live
/// suggest-tags endpoint runs IngredientFlavorHeuristic directly against
/// typed ingredient names, not persisted IngredientFlavorTag rows).
/// CocktailFlavorTagSyncService's own id/DB-join-based bulk implementation
/// is left as-is -- same math, no reason to risk regressing something that
/// already works at bulk scale.
/// </summary>
public static class CocktailFlavorTagHeuristic
{
    public static IReadOnlySet<string> ComputeProminentTags(IEnumerable<IReadOnlySet<string>> ingredientTagSets)
    {
        var counts = new Dictionary<string, int>();

        foreach (var tags in ingredientTagSets)
        {
            foreach (var tag in tags)
            {
                counts[tag] = counts.GetValueOrDefault(tag) + 1;
            }
        }

        if (counts.Count == 0)
        {
            return new HashSet<string>();
        }

        var max = counts.Values.Max();

        // "At least half of the max" -- IngredientCount * 2 >= max avoids
        // integer-division rounding, same rule CocktailFlavorTagSyncService
        // uses.
        return counts.Where(kv => kv.Value * 2 >= max).Select(kv => kv.Key).ToHashSet();
    }
}
