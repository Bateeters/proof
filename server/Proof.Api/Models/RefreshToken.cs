namespace Proof.Api.Models;

public class RefreshToken
{
    public Guid Id { get; set; }
    public required Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;

    // SHA-256 hash of the raw token -- only the hash is stored, same
    // principle as password hashing: if the DB is ever read, the raw
    // tokens (which are what the httpOnly cookie actually holds) aren't
    // directly sitting there usable. A fast hash (not BCrypt) is
    // appropriate here since the raw value is already a long random
    // string, not a low-entropy guessable password.
    public required string TokenHash { get; set; }

    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Null while active. Set when the token is used (rotation) or the
    // account logs out. A still-non-null RevokedAt on a token that's
    // presented again is a reuse signal -- see AuthController.Refresh.
    public DateTime? RevokedAt { get; set; }
}
