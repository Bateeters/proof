namespace Proof.Api.Models;

public class ProfileAllergen
{
    public Guid Id { get; set; }
    public required Guid ProfileId { get; set; }
    public Profile Profile { get; set; } = null!;
    public required string Name { get; set; }
}