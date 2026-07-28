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

    // Reverse of CocktailIngredient/CocktailSeason's "Cocktail" navigation —
    // lets us query/include a cocktail's own ingredients and seasons directly,
    // e.g. c.CocktailSeasons.Any(...) or .Include(c => c.CocktailIngredients).
    public ICollection<CocktailIngredient> CocktailIngredients { get; set; } = new List<CocktailIngredient>();
    public ICollection<CocktailSeason> CocktailSeasons { get; set; } = new List<CocktailSeason>();
}