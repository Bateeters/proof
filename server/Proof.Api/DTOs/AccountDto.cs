namespace Proof.Api.DTOs;

public class AccountDto
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}