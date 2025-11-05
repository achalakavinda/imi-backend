namespace MigratingAssistant.Application.Jobs.Queries.GetJobs;

public record GetJobsQuery : IRequest<List<JobDto>>;

public class GetJobsQueryHandler : IRequestHandler<GetJobsQuery, List<JobDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetJobsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<JobDto>> Handle(GetJobsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Jobs
            .AsNoTracking()
            .ProjectTo<JobDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
