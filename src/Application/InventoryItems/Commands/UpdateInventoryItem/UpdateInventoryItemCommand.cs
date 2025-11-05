namespace MigratingAssistant.Application.InventoryItems.Commands.UpdateInventoryItem;

public record UpdateInventoryItemCommand : IRequest
{
    public Guid Id { get; init; }
    public Guid ListingId { get; init; }
    public string? Sku { get; init; }
    public string? Metadata { get; init; }
    public bool Active { get; init; }
}

public class UpdateInventoryItemCommandHandler : IRequestHandler<UpdateInventoryItemCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateInventoryItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.InventoryItems
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(InventoryItem), request.Id.ToString());
        }

        entity.ListingId = request.ListingId;
        entity.Sku = request.Sku;
        entity.Metadata = request.Metadata;
        entity.Active = request.Active;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
