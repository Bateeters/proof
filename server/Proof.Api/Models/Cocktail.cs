namespace Proof.Api.Models;

public class Cocktail
{
    public Guid Id { get; set; }
    public string? ExternalId { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; }
    public required string Glass { get; set; }
    public required string Instructions { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsCustom { get; set; } = false;
    public Guid? OwnerProfileId { get; set; }
    public Profile? OwnerProfile { get; set; }

    // Who can see this cocktail -- only meaningful when IsCustom is true.
    public Visibility Visibility { get; set; } = Visibility.Private;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Soft delete only -- a custom cocktail's children (ingredients,
    // flavor tags, seasons) and anyone else's CookbookEntry referencing it
    // stay intact when "deleted". CocktailVisibility.VisibleTo is what
    // actually hides it from everyone except a profile that already saved
    // it before the delete.
    public bool IsDeleted { get; set; } = false;

    // Reverse of CocktailIngredient/CocktailSeason's "Cocktail" navigation —
    // lets us query/include a cocktail's own ingredients and seasons directly,
    // e.g. c.CocktailSeasons.Any(...) or .Include(c => c.CocktailIngredients).
    public ICollection<CocktailIngredient> CocktailIngredients { get; set; } = new List<CocktailIngredient>();
    public ICollection<CocktailSeason> CocktailSeasons { get; set; } = new List<CocktailSeason>();
    public ICollection<CocktailFlavorTag> CocktailFlavorTags { get; set; } = new List<CocktailFlavorTag>();
    public ICollection<CookbookEntry> CookbookEntries { get; set; } = new List<CookbookEntry>();
}