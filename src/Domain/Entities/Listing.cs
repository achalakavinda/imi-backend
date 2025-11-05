using MigratingAssistant.Domain.Enums;

namespace MigratingAssistant.Domain.Entities;

public class Listing : BaseAuditableEntity<Guid>
{
    public int ServiceTypeId { get; set; }
    public ServiceType? ServiceType { get; private set; }

    public Guid ProviderId { get; set; }
    public ServiceProvider? Provider { get; private set; }

    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Attributes { get; set; } // JSON as string
    public decimal? Price { get; set; }
    public string? Currency { get; set; } = "AUD";
    public ListingStatus Status { get; set; } = ListingStatus.Draft;
    public DateTimeOffset? AvailableFrom { get; set; }
    public DateTimeOffset? AvailableTo { get; set; }
    public int Version { get; set; } = 1;

    // Navigation properties
    public IList<InventoryItem> InventoryItems { get; private set; } = new List<InventoryItem>();
    public IList<Booking> Bookings { get; private set; } = new List<Booking>();
    public IList<Job> Jobs { get; private set; } = new List<Job>();
}
