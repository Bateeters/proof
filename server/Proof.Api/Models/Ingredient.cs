namespace Proof.Api.Models;

public class Ingredient
{
    public Guid Id { get; set; }
    public string? ExternalId { get; set; }
    public required string Name { get; set; }
    public IngredientType Type { get; set; }
    public CostTier CostTier { get; set; }
    public AvailabilityTier AvailabilityTier { get; set; }

    // Nullable — most ingredients (mixers, garnishes, syrups) aren't a base
    // spirit at all. Populated by IngredientSpiritHeuristic, same idea as
    // FlavorTags below but a single nullable link instead of a many-to-many
    // join, since an ingredient can only be at most one spirit.
    public Guid? SpiritId { get; set; }
    public Spirit? Spirit { get; set; }

    public ICollection<IngredientFlavorTag> FlavorTags { get; set; } = new List<IngredientFlavorTag>();
}