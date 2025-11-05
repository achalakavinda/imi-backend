using MigratingAssistant.Application.ServiceTypes.Queries;

namespace MigratingAssistant.Application.ServiceTypes.Queries.GetServiceTypeById;

public record GetServiceTypeByIdQuery(int Id) : IRequest<ServiceTypeDto>;

public class GetServiceTypeByIdQueryHandler : IRequestHandler<GetServiceTypeByIdQuery, ServiceTypeDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetServiceTypeByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ServiceTypeDto> Handle(GetServiceTypeByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.ServiceTypes
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .ProjectTo<ServiceTypeDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(ServiceType), request.Id.ToString());
        }

        return entity;
    }
}
