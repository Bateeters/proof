namespace Proof.Api.Models;

public class Cocktail
{
    public Guid Id { get; set; }
    public string? ExternalId { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; }
    public required string Glass { get; set; }
    public required string Instructions { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsCustom { get; set; } = false;
    public Guid? OwnerProfileId { get; set; }
    public Profile? OwnerProfile { get; set; }
}