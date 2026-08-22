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

    private static readonly string[] SmokeyKeywords =
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
}