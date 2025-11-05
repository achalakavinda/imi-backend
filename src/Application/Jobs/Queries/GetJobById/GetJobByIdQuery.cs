using MigratingAssistant.Application.Jobs.Queries;

namespace MigratingAssistant.Application.Jobs.Queries.GetJobById;

public record GetJobByIdQuery(Guid Id) : IRequest<JobDto>;

public class GetJobByIdQueryHandler : IRequestHandler<GetJobByIdQuery, JobDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetJobByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<JobDto> Handle(GetJobByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Jobs
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .ProjectTo<JobDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Job), request.Id.ToString());
        }

        return entity;
    }
}
