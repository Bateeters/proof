using Proof.Api.Models;

namespace Proof.Api.DTOs;

public class SpiritPreferenceDto
{
    public Guid SpiritId { get; set; }
    public required string SpiritName { get; set; }
    public Sentiment Sentiment { get; set; }
}