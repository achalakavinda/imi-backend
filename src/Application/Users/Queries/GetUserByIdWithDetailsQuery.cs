using MigratingAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MigratingAssistant.Application.Users.Queries;

public record GetUserByIdWithDetailsQuery(Guid Id) : IRequest<UserDto?>;

public class GetUserByIdWithDetailsQueryHandler : IRequestHandler<GetUserByIdWithDetailsQuery, UserDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetUserByIdWithDetailsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<UserDto?> Handle(GetUserByIdWithDetailsQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Profile)
            .Include(u => u.ServiceProvider)
            .Include(u => u.Documents)
            .Include(u => u.SupportTickets)
            .Include(u => u.Bookings)
                .ThenInclude(b => b.Payment)
            .Include(u => u.JobApplications)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        return user is null ? null : _mapper.Map<UserDto>(user);
    }
}