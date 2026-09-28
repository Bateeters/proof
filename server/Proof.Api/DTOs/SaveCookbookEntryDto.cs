namespace Proof.Api.DTOs;

public class SaveCookbookEntryDto
{
    public required Guid CocktailId { get; set; }
    public string? Notes { get; set; }
}
