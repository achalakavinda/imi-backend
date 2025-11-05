namespace MigratingAssistant.Application.InventoryItems.Commands.CreateInventoryItem;

public record CreateInventoryItemCommand : IRequest<Guid>
{
    public Guid ListingId { get; init; }
    public string? Sku { get; init; }
    public string? Metadata { get; init; }
    public bool Active { get; init; } = true;
}

public class CreateInventoryItemCommandHandler : IRequestHandler<CreateInventoryItemCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateInventoryItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var entity = new InventoryItem
        {
            ListingId = request.ListingId,
            Sku = request.Sku,
            Metadata = request.Metadata,
            Active = request.Active
        };

        _context.InventoryItems.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
