namespace Proof.Api.Models;

public class ProfileSpiritPreference
{
    public Guid Id { get; set; }
    public required Guid ProfileId { get; set; }
    public Profile Profile { get; set; } = null!;
    public required Guid SpiritId { get; set; }
    public Spirit Spirit { get; set; } = null!;
    public Sentiment Sentiment { get; set; }
}