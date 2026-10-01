namespace Proof.Api.DTOs;

public class RankedCocktailDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; }
    public required string Glass { get; set; }
    public string? ImageUrl { get; set; }
    public List<string> FlavorTags { get; set; } = [];

    // Signed; can go negative once dislikes/avoids outweigh likes/prefers.
    // Not normalized to a fixed range — it's a ranking key, not a percentage.
    public int MatchScore { get; set; }
}
