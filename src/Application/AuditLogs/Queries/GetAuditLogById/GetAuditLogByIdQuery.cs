using MigratingAssistant.Application.AuditLogs.Queries;

namespace MigratingAssistant.Application.AuditLogs.Queries.GetAuditLogById;

public record GetAuditLogByIdQuery(long Id) : IRequest<AuditLogDto>;

public class GetAuditLogByIdQueryHandler : IRequestHandler<GetAuditLogByIdQuery, AuditLogDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAuditLogByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<AuditLogDto> Handle(GetAuditLogByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.AuditLogs
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .ProjectTo<AuditLogDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(AuditLog), request.Id.ToString());
        }

        return entity;
    }
}
