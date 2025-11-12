namespace MigratingAssistant.Domain.Entities;

/// <summary>
/// Stores revoked access tokens to prevent their use even before expiration.
/// This is a blacklist approach for JWT token revocation.
/// </summary>
public class RevokedToken : BaseEntity
{
    public string Token { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public DateTime RevokedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? Reason { get; set; }
}
