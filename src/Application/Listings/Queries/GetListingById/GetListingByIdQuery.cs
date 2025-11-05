using MigratingAssistant.Application.Listings.Queries;

namespace MigratingAssistant.Application.Listings.Queries.GetListingById;

public record GetListingByIdQuery(Guid Id) : IRequest<ListingDto>;

public class GetListingByIdQueryHandler : IRequestHandler<GetListingByIdQuery, ListingDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetListingByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ListingDto> Handle(GetListingByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Listings
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .ProjectTo<ListingDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Listing), request.Id.ToString());
        }

        return entity;
    }
}
