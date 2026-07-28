namespace Proof.Api.DTOs;

public class CocktailDetailDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; }
    public required string Glass { get; set; }
    public string? ImageUrl { get; set; }
    public required string Instructions { get; set; }
    public required List<CocktailIngredientDto> Ingredients { get; set; }
}