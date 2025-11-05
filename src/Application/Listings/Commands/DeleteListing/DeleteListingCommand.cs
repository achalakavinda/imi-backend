namespace MigratingAssistant.Application.Listings.Commands.DeleteListing;

public record DeleteListingCommand(Guid Id) : IRequest;

public class DeleteListingCommandHandler : IRequestHandler<DeleteListingCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteListingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteListingCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Listings
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Listing), request.Id.ToString());
        }

        _context.Listings.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
