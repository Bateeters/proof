namespace Proof.Api.Models;

public class CookbookEntry
{
    public Guid Id { get; set; }
    public required Guid ProfileId { get; set; }
    public Profile Profile { get; set; } = null!;
    public required Guid CocktailId { get; set; }
    public Cocktail Cocktail { get; set; } = null!;
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
}
