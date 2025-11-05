namespace MigratingAssistant.Domain.Entities;

public class Document : BaseAuditableEntity<Guid>
{
    // Guid id will be inherited from BaseEntity<Guid>
    public Guid? UserId { get; set; }
    public User? User { get; private set; }

    public string? DocType { get; set; }
    public string? StoragePath { get; set; }
    public bool Verified { get; set; } = false;
    public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;
}
