namespace Proof.Api.DTOs;

// Shared shape for simple id/name lookup lists (Spirit, FlavorTag) — both
// are structurally identical (a seeded reference table with just a name),
// so one DTO covers both rather than two DTOs that would only ever differ
// in name.
public class LookupItemDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
}
