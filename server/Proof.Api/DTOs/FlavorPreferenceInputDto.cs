using Proof.Api.Models;

namespace Proof.Api.DTOs;

public class FlavorPreferenceInputDto
{
    public required Guid FlavorTagId { get; set; }
    public required Sentiment Sentiment { get; set; }
}