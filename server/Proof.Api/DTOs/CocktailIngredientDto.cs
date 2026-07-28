namespace Proof.Api.DTOs;

public class CocktailIngredientDto
{
    public required string IngredientName { get; set; }
    public string? Measure { get; set; }
}