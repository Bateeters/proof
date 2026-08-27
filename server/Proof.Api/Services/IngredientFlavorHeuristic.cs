using Proof.Api.Models;

namespace Proof.Api.Services;

public static class IngredientFlavorHeuristic
{
    private static readonly string[] SweetKeywords =
    {
        "sugar", "syrup", "honey", "grenadine", "agave", "butterscotch", "chocolate", "cocoa",
        "godiva", "marshmallows", "vanilla", "jello", "kahlua", "coffee liqueur", "coffee brandy",
        "tia maria", "caramel vodka"
    };

    private static readonly string[] SourKeywords =
    {
        "lemon", "lime", "sour", "vinegar", "cranberry"
    };

    private static readonly string[] BitterKeywords =
    {
        // "coffee" is deliberately excluded — it would also match "Coffee liqueur"/
        // "Coffee brandy", which are Sweet, not Bitter (same product category as Kahlua).
        // "espresso" doesn't share that problem since it's a different word entirely.
        "bitters", "campari", "aperol", "vermouth", "quinine", "tonic", "amaro montenegro",
        "dubonnet", "lillet", "espresso"
    };

    private static readonly string[] CitrusKeywords =
    {
        "lemon", "lime", "orange", "grapefruit", "citrus", "curacao", "cointreau", "grand marnier",
        "triple sec", "sprite", "mountain dew", "7-up", "lemonade", "limeade"
    };

    private static readonly string[] HerbalKeywords =
    {
        "mint", "basil", "thyme", "rosemary", "sage", "chartreuse", "absinthe", "gin", "anis",
        "anisette", "benedictine", "drambuie", "falernum", "galliano", "jagermeister",
        "jägermeister", "ouzo", "pernod", "ricard", "sambuca", "wormwood", "cardamom",
        "coriander", "cumin"
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
        "elderflower", "rose", "lavender", "violet", "hibiscus", "st. germain"
    };

    private static readonly string[] FruityKeywords =
    {
        "berry", "cherry", "peach", "pineapple", "mango", "strawberry", "raspberry", "banana",
        "apple", "coconut", "blackberry", "blackberries", "blackcurrant", "apricot", "fruit",
        "cherries", "strawberries", "raspberries", "cranberry", "grape", "kiwi", "papaya", "pomegranate",
        "cassis", "banane", "melon", "passoa"
    };

    private static readonly string[] CreamyKeywords =
    {
        "cream", "milk", "egg", "yogurt", "yoghurt", "ice-cream"
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