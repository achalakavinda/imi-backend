namespace MigratingAssistant.Domain.Entities;

public class RefreshToken : BaseAuditableEntity
{
    public string Token { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsRevoked && !IsExpired;
}
