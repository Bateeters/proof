namespace Proof.Api.Models;

public class IngredientSubstitution
{
    public Guid Id { get; set; }
    public required Guid SourceIngredientId { get; set; }
    public Ingredient SourceIngredient { get; set; } = null!;
    public required Guid ReplacementIngredientId { get; set; }
    public Ingredient ReplacementIngredient { get; set; } = null!;
    public SubstitutionReason Reason { get; set; }
    public string? Notes { get; set; }
}
