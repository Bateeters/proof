namespace Proof.Api.DTOs;

public class SubstitutionRequestDto
{
    public required Guid CocktailId { get; set; }
    public required Guid IngredientId { get; set; }

    // Deliberately plain strings, not the SubstitutionReason enum -- this is
    // the two-question UX flow's own vocabulary ("Taste" vs "Availability",
    // then "Cost" vs "Supply" only if Availability was picked), which the
    // controller maps onto SubstitutionReason. Keeping it separate from the
    // stored enum keeps the UI flow free to ask its questions in whatever
    // order/shape makes sense without being coupled to the data model.
    public required string Reason { get; set; }
    public string? SubReason { get; set; }
}
