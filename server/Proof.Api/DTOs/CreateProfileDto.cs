namespace Proof.Api.DTOs;

public class CreateProfileDto
{
    public required string DisplayName { get; set; }
    public string AvatarColor { get; set; } = "gray";
}