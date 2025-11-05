namespace MigratingAssistant.Application.InventoryItems.Queries;

public class InventoryItemDto
{
    public Guid Id { get; init; }
    public Guid ListingId { get; init; }
    public string? Sku { get; init; }
    public string? Metadata { get; init; }
    public bool Active { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<InventoryItem, InventoryItemDto>();
        }
    }
}
