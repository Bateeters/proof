namespace Proof.Api.DTOs;

public class SubstitutionSuggestionDto
{
    public required Guid ReplacementIngredientId { get; set; }
    public required string ReplacementIngredientName { get; set; }
    public string? Notes { get; set; }
}
