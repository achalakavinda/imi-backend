namespace MigratingAssistant.Domain.Entities;

public class InventoryItem : BaseEntity<Guid>
{
    public Guid ListingId { get; set; }
    public Listing? Listing { get; private set; }

    public string? Sku { get; set; }
    public string? Metadata { get; set; } // JSON as string
    public bool Active { get; set; } = true;

    // Navigation properties
    public IList<Booking> Bookings { get; private set; } = new List<Booking>();
}
