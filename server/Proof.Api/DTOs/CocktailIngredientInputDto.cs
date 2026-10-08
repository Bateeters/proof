namespace Proof.Api.DTOs;

// Exactly one of IngredientId/NewIngredientName must be set -- validated in
// CustomCocktailService.ResolveIngredientAsync, not via attributes (this
// codebase doesn't use DataAnnotations/FluentValidation anywhere). SpiritId
// is only meaningful alongside NewIngredientName: a manual override for
// when IngredientSpiritHeuristic doesn't recognize the typed name. SortOrder
// isn't here -- it's derived from array position server-side, not client-
// supplied.
public class CocktailIngredientInputDto
{
    public Guid? IngredientId { get; set; }
    public string? NewIngredientName { get; set; }
    public Guid? SpiritId { get; set; }
    public string? Measure { get; set; }
}
