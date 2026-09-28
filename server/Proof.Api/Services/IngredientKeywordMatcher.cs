namespace Proof.Api.Services;

/// <summary>
/// Shared word/phrase matching used by ingredient-classifying heuristics
/// (flavor tags, spirit identification). A single-word keyword must match a
/// whole word in the ingredient name (avoids "apple" matching inside
/// "Pineapple"); a multi-word keyword is matched as a substring of the whole
/// ingredient name instead, since a multi-word phrase doesn't have that same
/// false-positive risk.
/// </summary>
public static class IngredientKeywordMatcher
{
    public static bool MatchesAnyKeyword(string ingredient, string[] keywords)
    {
        var words = ingredient.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var keyword in keywords)
        {
            if (keyword.Contains(' '))
            {
                if (ingredient.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            else
            {
                if (words.Contains(keyword, StringComparer.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
