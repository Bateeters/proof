using Proof.Api.Models;

namespace Proof.Api.DTOs;

// CocktailSummaryDto's fields minus MatchScore (meaningless for a profile's
// own management view of their creations) plus Visibility.
public class MyCocktailSummaryDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; }
    public required string Glass { get; set; }
    public string? ImageUrl { get; set; }
    public List<string> FlavorTags { get; set; } = [];
    public required Visibility Visibility { get; set; }
}
