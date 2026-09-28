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

        if (IngredientKeywordMatcher.MatchesAnyKeyword(ingredientName, SweetKeywords))
        {
            flavorTag.Add("Sweet");
        }
        if (IngredientKeywordMatcher.MatchesAnyKeyword(ingredientName, SourKeywords))
        {
            flavorTag.Add("Sour");
        }
        if (IngredientKeywordMatcher.MatchesAnyKeyword(ingredientName, BitterKeywords))
        {
            flavorTag.Add("Bitter");
        }
        if (IngredientKeywordMatcher.MatchesAnyKeyword(ingredientName, CitrusKeywords))
        {
            flavorTag.Add("Citrus");
        }
        if (IngredientKeywordMatcher.MatchesAnyKeyword(ingredientName, HerbalKeywords))
        {
            flavorTag.Add("Herbal");
        }
        if (IngredientKeywordMatcher.MatchesAnyKeyword(ingredientName, SpicyKeywords))
        {
            flavorTag.Add("Spicy");
        }
        if (IngredientKeywordMatcher.MatchesAnyKeyword(ingredientName, SpicedKeywords))
        {
            flavorTag.Add("Spiced");
        }
        if (IngredientKeywordMatcher.MatchesAnyKeyword(ingredientName, SmokyKeywords))
        {
            flavorTag.Add("Smoky");
        }
        if (IngredientKeywordMatcher.MatchesAnyKeyword(ingredientName, FloralKeywords))
        {
            flavorTag.Add("Floral");
        }
        if (IngredientKeywordMatcher.MatchesAnyKeyword(ingredientName, FruityKeywords))
        {
            flavorTag.Add("Fruity");
        }
        if (IngredientKeywordMatcher.MatchesAnyKeyword(ingredientName, CreamyKeywords))
        {
            flavorTag.Add("Creamy");
        }
        if (IngredientKeywordMatcher.MatchesAnyKeyword(ingredientName, NuttyKeywords))
        {
            flavorTag.Add("Nutty");
        }
        if (IngredientKeywordMatcher.MatchesAnyKeyword(ingredientName, RefreshingKeywords))
        {
            flavorTag.Add("Refreshing");
        }

        return flavorTag;
    }
}