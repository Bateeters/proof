namespace Proof.Api.Models;

public class CocktailFlavorTag
{
    public Guid Id { get; set; }
    public required Guid CocktailId { get; set; }
    public Cocktail Cocktail { get; set; } = null!;
    public required Guid FlavorTagId { get; set; }
    public FlavorTag FlavorTag { get; set; } = null!;
}