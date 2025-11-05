using MigratingAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MigratingAssistant.Application.JobApplications.Queries;

public record GetJobApplicationsQuery : IRequest<List<JobApplicationDto>>;

public class GetJobApplicationsQueryHandler : IRequestHandler<GetJobApplicationsQuery, List<JobApplicationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetJobApplicationsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<JobApplicationDto>> Handle(GetJobApplicationsQuery request, CancellationToken cancellationToken)
    {
        var applications = await _context.JobApplications
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<JobApplicationDto>>(applications);
    }
}