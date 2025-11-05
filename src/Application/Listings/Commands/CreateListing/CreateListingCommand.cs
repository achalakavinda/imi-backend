namespace MigratingAssistant.Application.Listings.Commands.CreateListing;

public record CreateListingCommand : IRequest<Guid>
{
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

public class CreateListingCommandHandler : IRequestHandler<CreateListingCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateListingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateListingCommand request, CancellationToken cancellationToken)
    {
        var entity = new Listing
        {
            ServiceTypeId = request.ServiceTypeId,
            ProviderId = request.ProviderId,
            Title = request.Title,
            Description = request.Description,
            Attributes = request.Attributes,
            Price = request.Price,
            Currency = request.Currency ?? "AUD",
            Status = (Domain.Enums.ListingStatus)request.Status,
            AvailableFrom = request.AvailableFrom,
            AvailableTo = request.AvailableTo
        };

        _context.Listings.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
