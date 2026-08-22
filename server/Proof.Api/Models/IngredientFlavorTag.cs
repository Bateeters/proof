namespace Proof.Api.Models;

public class IngredientFlavorTag
{
    public Guid Id { get; set; }
    public required Guid IngredientId { get; set; }
    public Ingredient Ingredient { get; set; } = null!;
    public required Guid FlavorTagId { get; set; }
    public FlavorTag FlavorTag { get; set; } = null!;
}