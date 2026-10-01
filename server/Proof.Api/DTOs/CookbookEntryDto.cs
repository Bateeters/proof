namespace Proof.Api.DTOs;

public class CookbookEntryDto
{
    public required Guid CocktailId { get; set; }
    public required string CocktailName { get; set; }
    public required string CocktailCategory { get; set; }
    public string? CocktailImageUrl { get; set; }
    public List<string> FlavorTags { get; set; } = [];
    public DateTime SavedAt { get; set; }
    public string? Notes { get; set; }
}
