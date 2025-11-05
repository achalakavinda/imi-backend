using MigratingAssistant.Application.AuditLogs.Queries;

namespace MigratingAssistant.Application.AuditLogs.Queries.GetAuditLogs;

public record GetAuditLogsQuery : IRequest<List<AuditLogDto>>;

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, List<AuditLogDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAuditLogsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ProjectTo<AuditLogDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
