namespace Proof.Api.Models;

public class CocktailIngredient
{
    public Guid Id { get; set; }
    public required Guid CocktailId { get; set; }
    public Cocktail Cocktail { get; set; } = null!;
    public required Guid IngredientId { get; set; }
    public Ingredient Ingredient { get; set; } = null!;
    public string? Measure { get; set; }
    public int SortOrder { get; set; }
}