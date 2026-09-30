namespace Proof.Api.DTOs;

public class CategoryPreviewDto
{
    public required string Category { get; set; }
    public required List<CocktailSummaryDto> Cocktails { get; set; }
}
