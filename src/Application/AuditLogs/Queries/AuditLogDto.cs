namespace MigratingAssistant.Application.AuditLogs.Queries;

public class AuditLogDto
{
    public long Id { get; init; }
    public string? Entity { get; init; }
    public Guid? EntityId { get; init; }
    public string? Action { get; init; }
    public string? Payload { get; init; }
    public DateTimeOffset CreatedAt { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<AuditLog, AuditLogDto>();
        }
    }
}
