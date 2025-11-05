namespace MigratingAssistant.Application.Listings.Commands.UpdateListing;

public record UpdateListingCommand : IRequest
{
    public Guid Id { get; init; }
    public int ServiceTypeId { get; init; }
    public Guid ProviderId { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? Attributes { get; init; }
    public decimal? Price { get; init; }
    public string? Currency { get; init; }
    public int Status { get; init; }
    public DateTimeOffset? AvailableFrom { get; init; }
    public DateTimeOffset? AvailableTo { get; init; }
}

public class UpdateListingCommandHandler : IRequestHandler<UpdateListingCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateListingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateListingCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Listings
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Listing), request.Id.ToString());
        }

        entity.ServiceTypeId = request.ServiceTypeId;
        entity.ProviderId = request.ProviderId;
        entity.Title = request.Title;
        entity.Description = request.Description;
        entity.Attributes = request.Attributes;
        entity.Price = request.Price;
        entity.Currency = request.Currency;
        entity.Status = (Domain.Enums.ListingStatus)request.Status;
        entity.AvailableFrom = request.AvailableFrom;
        entity.AvailableTo = request.AvailableTo;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
