namespace Proof.Api.DTOs;

public class CocktailIngredientDto
{
    public required Guid IngredientId { get; set; }
    public required string IngredientName { get; set; }
    public string? Measure { get; set; }
}