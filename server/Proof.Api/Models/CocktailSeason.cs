namespace Proof.Api.Models;

public class CocktailSeason
{
    public Guid Id { get; set; }
    public required Guid CocktailId { get; set; }
    public Cocktail Cocktail { get; set; } = null!;
    public Season Season { get; set; }
}