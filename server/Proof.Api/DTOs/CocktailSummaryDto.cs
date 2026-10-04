namespace Proof.Api.DTOs;

public class CocktailSummaryDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; }
    public required string Glass { get; set; }
    public string? ImageUrl { get; set; }
    public List<string> FlavorTags { get; set; } = [];

    // Signed; can go negative once dislikes/avoids outweigh likes/prefers.
    // Not normalized to a fixed range -- it's a ranking key, not a
    // percentage. Stays 0 for every cocktail when no profile context is
    // given, which the sort's alphabetical tiebreaker reduces to a plain
    // A-Z order. Meaning depends on which service produced the list:
    // TasteRankingService scores "how much this profile would like it";
    // CocktailSimilarityService scores "how much this has in common with
    // another cocktail" (flavor/spirit/category overlap). Same shape,
    // different ranking signal -- callers that display the raw number
    // (e.g. Home's "Recommended For You" subtitle) should only do so where
    // the meaning is unambiguous from context.
    public int MatchScore { get; set; }
}