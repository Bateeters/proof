namespace Proof.Api.Models;

public class Ingredient
{
    public Guid Id { get; set; }
    public string? ExternalId { get; set; }
    public required string Name { get; set; }
    public IngredientType Type { get; set; }
    public CostTier CostTier { get; set; }
    public AvailabilityTier AvailabilityTier { get; set; }
}