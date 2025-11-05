namespace MigratingAssistant.Application.InventoryItems.Commands.DeleteInventoryItem;

public record DeleteInventoryItemCommand(Guid Id) : IRequest;

public class DeleteInventoryItemCommandHandler : IRequestHandler<DeleteInventoryItemCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteInventoryItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.InventoryItems
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(InventoryItem), request.Id.ToString());
        }

        _context.InventoryItems.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
