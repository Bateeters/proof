using Proof.Api.Models;

namespace Proof.Api.DTOs;

public class SuggestTagsIngredientDto
{
    public Guid? IngredientId { get; set; }
    public string? NewIngredientName { get; set; }
}

public class SuggestTagsRequestDto
{
    public required List<SuggestTagsIngredientDto> Ingredients { get; set; }
}

public class SuggestTagsResponseDto
{
    public List<Guid> FlavorTagIds { get; set; } = [];
    public List<Season> Seasons { get; set; } = [];
}
