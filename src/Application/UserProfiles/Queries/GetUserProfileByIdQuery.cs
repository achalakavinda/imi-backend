using MigratingAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MigratingAssistant.Application.UserProfiles.Queries;

public record GetUserProfileByIdQuery(Guid Id) : IRequest<UserProfileDto?>;

public class GetUserProfileByIdQueryHandler : IRequestHandler<GetUserProfileByIdQuery, UserProfileDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetUserProfileByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<UserProfileDto?> Handle(GetUserProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var profile = await _context.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        return profile is null ? null : _mapper.Map<UserProfileDto>(profile);
    }
}