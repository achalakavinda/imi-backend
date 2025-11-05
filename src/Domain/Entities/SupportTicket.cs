using MigratingAssistant.Domain.Enums;

namespace MigratingAssistant.Domain.Entities;

public class SupportTicket : BaseAuditableEntity<Guid>
{
    // Guid id will be inherited from BaseEntity<Guid>
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public string? Subject { get; set; }
    public string? Body { get; set; }
    public SupportTicketStatus Status { get; set; } = SupportTicketStatus.Open;
}
