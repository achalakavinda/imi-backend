namespace MigratingAssistant.Application.ServiceTypes.Queries.GetServiceTypes;

public record GetServiceTypesQuery : IRequest<List<ServiceTypeDto>>;

public class GetServiceTypesQueryHandler : IRequestHandler<GetServiceTypesQuery, List<ServiceTypeDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetServiceTypesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ServiceTypeDto>> Handle(GetServiceTypesQuery request, CancellationToken cancellationToken)
    {
        return await _context.ServiceTypes
            .AsNoTracking()
            .ProjectTo<ServiceTypeDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
