namespace Proof.Api.Services;

/// <summary>
/// Matches a profile's free-text allergen entries (e.g. "dairy", "tree nuts")
/// against real ingredient names, for the hard-exclusion safety filter in
/// taste ranking. This is a best-effort match, not a guarantee — see the
/// disclaimer note in ARCHITECTURE.md. Unlike the other heuristics, this
/// isn't precomputed/synced: ProfileAllergen is free text that can change at
/// any time and has no fixed vocabulary, so matching happens live at ranking
/// time against whatever text a profile actually entered.
/// </summary>
public static class AllergenHeuristic
{
    // Curated only for allergen words that don't literally appear inside the
    // ingredient names they're relevant to (e.g. "dairy" never appears in
    // "Cream" or "Milk", so a direct-text match alone would miss it). An
    // allergen entry that isn't one of these categories (or a synonym of
    // one) falls back to matching its own text directly against ingredient
    // names instead — e.g. "coconut" or "soy" don't need a category here,
    // since "Coconut" and "Soy Sauce" already contain those exact words.
    private static readonly Dictionary<string, string[]> CategoryKeywordsByName = new(StringComparer.OrdinalIgnoreCase)
    {
        ["dairy"] = ["cream", "milk", "egg", "yogurt", "yoghurt", "half-and-half", "ice-cream"],
        ["milk"] = ["cream", "milk", "egg", "yogurt", "yoghurt", "half-and-half", "ice-cream"],
        ["lactose"] = ["cream", "milk", "yogurt", "yoghurt", "half-and-half", "ice-cream"],
        ["nuts"] = ["almond", "hazelnut", "walnut", "peanut", "amaretto", "orgeat", "frangelico"],
        ["nut"] = ["almond", "hazelnut", "walnut", "peanut", "amaretto", "orgeat", "frangelico"],
        ["tree nuts"] = ["almond", "hazelnut", "walnut", "amaretto", "orgeat", "frangelico"],
        ["gluten"] = ["beer", "lager", "ale", "stout", "guinness"],
        ["wheat"] = ["beer", "lager", "ale", "stout", "guinness"],
        ["barley"] = ["beer", "lager", "ale", "stout", "guinness"],
        ["sulfites"] = ["wine", "champagne", "prosecco", "sherry", "port"],
        ["sulfite"] = ["wine", "champagne", "prosecco", "sherry", "port"]
    };

    public static bool IngredientMatchesAllergen(string ingredientName, string allergenText)
    {
        var normalized = allergenText.Trim();

        if (normalized.Length == 0)
        {
            return false;
        }

        var keywords = CategoryKeywordsByName.TryGetValue(normalized, out var categoryKeywords)
            ? categoryKeywords
            : [normalized];

        return IngredientKeywordMatcher.MatchesAnyKeyword(ingredientName, keywords);
    }
}
