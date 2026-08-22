using Proof.Api.Models;

namespace Proof.Api.DTOs;

public class SpiritPreferenceInputDto
{
    public required Guid SpiritId { get; set; }
    public required Sentiment Sentiment { get; set; }
}