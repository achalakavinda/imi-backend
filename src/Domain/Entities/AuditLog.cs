namespace MigratingAssistant.Domain.Entities;

public class AuditLog : BaseEntity<long>
{
    public string? Entity { get; set; }
    public Guid? EntityId { get; set; }
    public string? Action { get; set; }
    public string? Payload { get; set; } // JSON as string
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
