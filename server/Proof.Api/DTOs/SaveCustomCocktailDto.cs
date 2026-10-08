using Proof.Api.Models;

namespace Proof.Api.DTOs;

public class SaveCustomCocktailDto
{
    public required string Name { get; set; }
    public required string Category { get; set; }
    public required string Glass { get; set; }
    public required string Instructions { get; set; }
    public string? ImageUrl { get; set; }
    public required Visibility Visibility { get; set; }
    public required List<CocktailIngredientInputDto> Ingredients { get; set; }
    public List<Guid> FlavorTagIds { get; set; } = [];
    public List<Season> Seasons { get; set; } = [];
}
