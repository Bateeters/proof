using Proof.Api.Models;

namespace Proof.Api.Services;

public static class IngredientFlavorHeuristic
{
    private static readonly string[] SweetKeywords =
    {
        "sugar", "syrup", "honey", "grenadine", "agave"
    };

    private static readonly string[] SourKeywords =
    {
        "lemon", "lime", "sour", "vinegar"
    };

    private static readonly string[] BitterKeywords =
    {
        "bitters", "campari", "aperol", "vermouth", "quinine", "tonic"
    };

    private static readonly string[] CitrusKeywords =
    {
        "lemon", "lime", "orange", "grapefruit", "citrus"
    };

    private static readonly string[] HerbalKeywords =
    {
        "mint", "basil", "thyme", "rosemary", "sage", "chartreuse", "absinthe", "gin"
    };

    private static readonly string[] SpicyKeywords =
    {
        "pepper", "chili", "jalapeno", "hot sauce"
    };

    private static readonly string[] SpicedKeywords =
    {
        "ginger", "cinnamon", "nutmeg", "clove", "allspice", "star anise"
    };

    private static readonly string[] SmokyKeywords =
    {
        "mezcal", "scotch", "smoked", "bacon"
    };

    private static readonly string[] FloralKeywords =
    {
        "elderflower", "rose", "lavender", "violet", "hibiscus"
    };

    private static readonly string[] FruityKeywords =
    {
        "berry", "cherry", "peach", "pineapple", "mango", "strawberry", "raspberry", "banana", "apple", "coconut"
    };

    private static readonly string[] CreamyKeywords =
    {
        "cream", "milk", "egg", "yogurt"
    };

    private static readonly string[] NuttyKeywords =
    {
        "almond", "hazelnut", "walnut", "amaretto", "peanut", "orgeat"
    };

    private static readonly string[] RefreshingKeywords =
    {
        "soda", "tonic", "water", "cucumber", "sparkling"
    };

    public static IReadOnlySet<string> AssignFlavorTag(string ingredientName)
    {
        var flavorTag = new HashSet<string>();

        if (MatchesAnyKeyword(ingredientName, SweetKeywords))
        {
            flavorTag.Add("Sweet");
        }
        if (MatchesAnyKeyword(ingredientName, SourKeywords))
        {
            flavorTag.Add("Sour");
        }
        if (MatchesAnyKeyword(ingredientName, BitterKeywords))
        {
            flavorTag.Add("Bitter");
        }
        if (MatchesAnyKeyword(ingredientName, CitrusKeywords))
        {
            flavorTag.Add("Citrus");
        }
        if (MatchesAnyKeyword(ingredientName, HerbalKeywords))
        {
            flavorTag.Add("Herbal");
        }
        if (MatchesAnyKeyword(ingredientName, SpicyKeywords))
        {
            flavorTag.Add("Spicy");
        }
        if (MatchesAnyKeyword(ingredientName, SpicedKeywords))
        {
            flavorTag.Add("Spiced");
        }
        if (MatchesAnyKeyword(ingredientName, SmokyKeywords))
        {
            flavorTag.Add("Smoky");
        }
        if (MatchesAnyKeyword(ingredientName, FloralKeywords))
        {
            flavorTag.Add("Floral");
        }
        if (MatchesAnyKeyword(ingredientName, FruityKeywords))
        {
            flavorTag.Add("Fruity");
        }
        if (MatchesAnyKeyword(ingredientName, CreamyKeywords))
        {
            flavorTag.Add("Creamy");
        }
        if (MatchesAnyKeyword(ingredientName, NuttyKeywords))
        {
            flavorTag.Add("Nutty");
        }
        if (MatchesAnyKeyword(ingredientName, RefreshingKeywords))
        {
            flavorTag.Add("Refreshing");
        }

        return flavorTag;
    }

    private static bool MatchesAnyKeyword(string ingredient, string[] keywords)
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