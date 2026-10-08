using Proof.Api.Models;

namespace Proof.Api.DTOs;

public class CocktailDetailDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; }
    public required string Glass { get; set; }
    public string? ImageUrl { get; set; }
    public required string Instructions { get; set; }
    public required List<CocktailIngredientDto> Ingredients { get; set; }
    public List<string> FlavorTags { get; set; } = [];
    // Ids alongside names -- FlavorTag is an id-based lookup entity (unlike
    // Season, a plain enum with no separate id concept), and the edit form
    // needs ids to pre-check its flavor-tag multi-select against the same
    // LookupItem[] it fetches from /api/lookup/flavor-tags.
    public List<Guid> FlavorTagIds { get; set; } = [];
    public List<string> Seasons { get; set; } = [];
    public bool IsCustom { get; set; }
    public Visibility Visibility { get; set; }
    public bool IsOwnedByCaller { get; set; }
}