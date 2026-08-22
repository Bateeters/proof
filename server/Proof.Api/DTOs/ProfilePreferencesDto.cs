namespace Proof.Api.DTOs;

public class ProfilePreferencesDto
{
    public required List<SpiritPreferenceDto> SpiritPreferences { get; set; }
    public required List<FlavorPreferenceDto> FlavorPreferences { get; set; }
    public required List<string> Allergens { get; set; }
}