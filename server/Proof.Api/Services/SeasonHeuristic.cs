using Proof.Api.Models;

namespace Proof.Api.Services;

public static class SeasonHeuristic
{
    private static readonly string[] SpringKeywords =
    {
        "gin", "elderflower", "champagne", "prosecco", "strawberry", "berry", "berries", "cucumber", "ginger", "rhubarb"
    };

    private static readonly string[] SummerKeywords =
    {
        "rum", "tequila", "lime", "lemon", "pineapple", "coconut", "watermelon", "mint", "tropical"
    };

    private static readonly string[] FallKeywords =
    {
        "whiskey", "bourbon", "apple", "cinnamon", "nutmeg", "clove", "maple", "pumpkin", "allspice"
    };

    private static readonly string[] WinterKeywords =
    {
        "whiskey", "bourbon", "brandy", "cognac", "cinnamon", "nutmeg", "clove", "cocoa", "coffee", "cream", "egg", "peppermint"
    };

    public static IReadOnlySet<Season> AssignSeasons(IEnumerable<string> ingredientNames)
    {
        var seasons = new HashSet<Season>();

        foreach (var ingredient in ingredientNames)
        {
            if (MatchesAnyKeyword(ingredient, SpringKeywords))
            {
                seasons.Add(Season.Spring);
            }

            if (MatchesAnyKeyword(ingredient, SummerKeywords))
            {
                seasons.Add(Season.Summer);
            }

            if (MatchesAnyKeyword(ingredient, FallKeywords))
            {
                seasons.Add(Season.Fall);
            }

            if (MatchesAnyKeyword(ingredient, WinterKeywords))
            {
                seasons.Add(Season.Winter);
            }
        }

        if (seasons.Count == 0)
        {
            seasons.UnionWith(Enum.GetValues<Season>());
        }

        return seasons;
    }

    private static bool MatchesAnyKeyword(string ingredient, string[] keywords)
    {
        var words = ingredient.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.Any(words => keywords.Contains(words, StringComparer.OrdinalIgnoreCase));
    }
}