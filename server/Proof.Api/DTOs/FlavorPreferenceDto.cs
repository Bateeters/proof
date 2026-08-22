using Proof.Api.Models;

namespace Proof.Api.DTOs;

public class FlavorPreferenceDto
{
    public Guid FlavorTagId { get; set; }
    public required string FlavorTagName { get; set; }
    public Sentiment Sentiment { get; set; }
}