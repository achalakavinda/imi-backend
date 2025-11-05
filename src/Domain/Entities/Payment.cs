using MigratingAssistant.Domain.Enums;

namespace MigratingAssistant.Domain.Entities;

public class Payment : BaseAuditableEntity<Guid>
{
    // Guid id will be inherited from BaseEntity<Guid>
    public Guid UserId { get; set; }
    public User? User { get; private set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "AUD";
    public string? GatewayReference { get; set; }
    public PaymentGatewayStatus Status { get; set; } = PaymentGatewayStatus.Initiated;
    public string? Meta { get; set; } // JSON as string
    public string? IdempotencyKey { get; set; }
}
