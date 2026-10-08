namespace Proof.Api.Models;

public enum IngredientType
{
    Spirit,
    Mixer,
    Garnish,
    Bitters,
    Syrup,
    Other
}

public enum CostTier
{
    Budget,
    Mid,
    Splurge
}

public enum AvailabilityTier
{
    Common,
    Specialty,
    RareHardToFind,
    Seasonal
}

public enum Season
{
    Spring,
    Summer,
    Fall,
    Winter
}

public enum Sentiment
{
    Positive,
    Negative
}

public enum SubstitutionReason
{
    Taste,
    Cost,
    Supply
}

// Who can see a custom cocktail. Only meaningful when Cocktail.IsCustom is
// true -- synced catalog cocktails carry the default value but it's never
// consulted for them (CocktailVisibility.VisibleTo short-circuits on
// !IsCustom first). Private: only the creating profile. Local: every
// profile under the same account. Global: everyone.
public enum Visibility
{
    Private,
    Local,
    Global
}