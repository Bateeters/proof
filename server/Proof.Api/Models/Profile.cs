namespace Proof.Api.Models;

public class Profile
{
    public Guid Id { get; set; }
    public required Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public required string DisplayName { get; set; }
    public required string AvatarColor { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ProfileSpiritPreference> SpiritPreferences { get; set; } = new List<ProfileSpiritPreference>();
    public ICollection<ProfileFlavorPreference> FlavorPreferences { get; set; } = new List<ProfileFlavorPreference>();
    public ICollection<ProfileAllergen> Allergens { get; set; } = new List<ProfileAllergen>();
}