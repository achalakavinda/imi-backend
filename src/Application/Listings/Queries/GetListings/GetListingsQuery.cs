namespace MigratingAssistant.Application.Listings.Queries.GetListings;

public record GetListingsQuery : IRequest<List<ListingDto>>;

public class GetListingsQueryHandler : IRequestHandler<GetListingsQuery, List<ListingDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetListingsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ListingDto>> Handle(GetListingsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Listings
            .AsNoTracking()
            .ProjectTo<ListingDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
