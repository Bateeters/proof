namespace Proof.Api.DTOs;

public class WhatCanIMakeResultDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; }
    public required string Glass { get; set; }
    public string? ImageUrl { get; set; }

    // Empty means fully makeable with what was listed. Ranked ascending by
    // this list's length — "closeness" is literally "how few ingredients
    // are missing," the simplest honest definition that also naturally
    // surfaces exact matches first.
    public required List<string> MissingIngredients { get; set; }
}
