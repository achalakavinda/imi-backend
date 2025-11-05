using MigratingAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MigratingAssistant.Application.Users.Queries;

public record GetUsersWithDetailsQuery : IRequest<List<UserDto>>;

public class GetUsersWithDetailsQueryHandler : IRequestHandler<GetUsersWithDetailsQuery, List<UserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetUsersWithDetailsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<UserDto>> Handle(GetUsersWithDetailsQuery request, CancellationToken cancellationToken)
    {
        var users = await _context.Users
            .Include(u => u.Profile)
            .Include(u => u.ServiceProvider)
            .Include(u => u.Documents)
            .Include(u => u.SupportTickets)
            .Include(u => u.Bookings)
                .ThenInclude(b => b.Payment)
            .Include(u => u.JobApplications)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<UserDto>>(users);
    }
}