using MigratingAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MigratingAssistant.Application.JobApplications.Queries;

public record GetJobApplicationByIdQuery(Guid Id) : IRequest<JobApplicationDto?>;

public class GetJobApplicationByIdQueryHandler : IRequestHandler<GetJobApplicationByIdQuery, JobApplicationDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetJobApplicationByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<JobApplicationDto?> Handle(GetJobApplicationByIdQuery request, CancellationToken cancellationToken)
    {
        var application = await _context.JobApplications
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == request.Id, cancellationToken);

        return application is null ? null : _mapper.Map<JobApplicationDto>(application);
    }
}