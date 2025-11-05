using MigratingAssistant.Domain.Enums;

namespace MigratingAssistant.Domain.Entities;

public class Booking : BaseAuditableEntity<Guid>
{
    // Guid id will be inherited from BaseEntity<Guid>
    public Guid ListingId { get; set; }
    public Listing? Listing { get; private set; }

    public Guid? InventoryItemId { get; set; }
    public InventoryItem? InventoryItem { get; private set; }

    public Guid UserId { get; set; }
    public User? User { get; private set; }

    public DateTimeOffset? StartAt { get; set; }
    public DateTimeOffset? EndAt { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public Guid? PaymentId { get; set; }
    public Payment? Payment { get; private set; }

    public string? IdempotencyKey { get; set; }
    public int Version { get; set; } = 1;
}
