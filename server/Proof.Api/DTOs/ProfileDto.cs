namespace Proof.Api.DTOs;

public class ProfileDto
{
    public Guid Id { get; set; }
    public required string DisplayName { get; set; }
    public required string AvatarColor { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}