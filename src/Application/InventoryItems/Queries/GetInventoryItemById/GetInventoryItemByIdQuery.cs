using MigratingAssistant.Application.InventoryItems.Queries;

namespace MigratingAssistant.Application.InventoryItems.Queries.GetInventoryItemById;

public record GetInventoryItemByIdQuery(Guid Id) : IRequest<InventoryItemDto>;

public class GetInventoryItemByIdQueryHandler : IRequestHandler<GetInventoryItemByIdQuery, InventoryItemDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetInventoryItemByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<InventoryItemDto> Handle(GetInventoryItemByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.InventoryItems
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .ProjectTo<InventoryItemDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(InventoryItem), request.Id.ToString());
        }

        return entity;
    }
}
