namespace MigratingAssistant.Application.InventoryItems.Queries.GetInventoryItems;

public record GetInventoryItemsQuery : IRequest<List<InventoryItemDto>>;

public class GetInventoryItemsQueryHandler : IRequestHandler<GetInventoryItemsQuery, List<InventoryItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetInventoryItemsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<InventoryItemDto>> Handle(GetInventoryItemsQuery request, CancellationToken cancellationToken)
    {
        return await _context.InventoryItems
            .AsNoTracking()
            .ProjectTo<InventoryItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
