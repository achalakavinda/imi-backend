using MigratingAssistant.Domain.Enums;

namespace MigratingAssistant.Domain.Entities;

public class ServiceType : BaseEntity<int>
{
    public string ServiceKey { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? SchemaHint { get; set; } // JSON as string
    public bool Enabled { get; set; } = true;

    // Navigation properties
    public IList<Listing> Listings { get; private set; } = new List<Listing>();
}
