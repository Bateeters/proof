namespace Proof.Api.DTOs;

public class UpdateProfilePreferencesDto
{
    public required List<SpiritPreferenceInputDto> SpiritPreferences { get; set; }
    public required List<FlavorPreferenceInputDto> FlavorPreferences { get; set; }
    public required List<string> Allergens { get; set; }
}