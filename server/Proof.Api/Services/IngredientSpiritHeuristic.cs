namespace Proof.Api.Services;

/// <summary>
/// Identifies which base <see cref="Models.Spirit"/> (if any) an ingredient
/// is, so taste ranking can connect a cocktail's actual ingredients (e.g.
/// "Coconut rum") back to a profile's spirit preferences (e.g. "Likes Rum").
/// Unlike <see cref="IngredientFlavorHeuristic"/>, an ingredient can only be
/// ONE spirit at most, so this returns a single nullable name, not a set.
/// </summary>
public static class IngredientSpiritHeuristic
{
    private static readonly string[] BourbonKeywords =
    {
        "bourbon", "jim beam", "wild turkey"
    };

    private static readonly string[] ScotchKeywords =
    {
        "scotch"
    };

    private static readonly string[] CognacKeywords =
    {
        "cognac"
    };

    private static readonly string[] TripleSecKeywords =
    {
        "triple sec"
    };

    private static readonly string[] WhiskeyKeywords =
    {
        // Covers "Whiskey"/"Whisky", "Blended whiskey", "Irish whiskey",
        // "Rye Whiskey", "Tennessee whiskey" (all contain "whiskey"/"whisky"
        // as a word already, no separate entries needed) plus brand names
        // that don't literally say "whiskey".
        "whiskey", "whisky", "jack daniels", "crown royal", "yukon jack", "southern comfort"
    };

    private static readonly string[] VodkaKeywords =
    {
        "vodka", "absolut citron", "absolut kurant", "absolut peppar",
        // Neutral grain spirits — closest existing category to unflavored,
        // high-proof vodka, not a perfect match but the best available one.
        "everclear", "grain alcohol"
    };

    private static readonly string[] GinKeywords =
    {
        "gin"
    };

    private static readonly string[] RumKeywords =
    {
        // Covers "Dark/Light/White/Gold/Spiced rum", "blackstrap rum",
        // "151 proof rum", "Añejo rum" (all contain "rum" as a word).
        "rum", "bacardi limon"
    };

    private static readonly string[] TequilaKeywords =
    {
        "tequila"
    };

    private static readonly string[] BrandyKeywords =
    {
        "brandy", "pisco"
    };

    private static readonly string[] MezcalKeywords =
    {
        "mezcal"
    };

    private static readonly string[] VermouthKeywords =
    {
        "vermouth"
    };

    // Order matters: a more specific spirit (Bourbon, Scotch, Cognac) is
    // checked before the generic bucket it could otherwise fall into
    // (Whiskey, Brandy) — in practice real ingredient names rarely collide
    // this way, but the order keeps the specific category as the tiebreaker
    // if one ever does.
    private static readonly (string SpiritName, string[] Keywords)[] SpiritsInPriorityOrder =
    {
        ("Bourbon", BourbonKeywords),
        ("Scotch", ScotchKeywords),
        ("Cognac", CognacKeywords),
        ("Triple Sec", TripleSecKeywords),
        ("Whiskey", WhiskeyKeywords),
        ("Vodka", VodkaKeywords),
        ("Gin", GinKeywords),
        ("Rum", RumKeywords),
        ("Tequila", TequilaKeywords),
        ("Brandy", BrandyKeywords),
        ("Mezcal", MezcalKeywords),
        ("Vermouth", VermouthKeywords)
    };

    public static string? IdentifySpirit(string ingredientName)
    {
        foreach (var (spiritName, keywords) in SpiritsInPriorityOrder)
        {
            if (IngredientKeywordMatcher.MatchesAnyKeyword(ingredientName, keywords))
            {
                return spiritName;
            }
        }

        return null;
    }
}
