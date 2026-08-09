namespace Proof.Api.Models;

public class ProfileFlavorPreference
{
    public Guid Id { get; set; }
    public required Guid ProfileId { get; set; }
    public Profile Profile { get; set; } = null!;
    public required Guid FlavorTagId { get; set; }
    public FlavorTag FlavorTag { get; set; } = null!;
    public Sentiment Sentiment { get; set; }
}