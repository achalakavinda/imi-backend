using MigratingAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MigratingAssistant.Application.UserProfiles.Queries;

public record GetUserProfilesQuery : IRequest<List<UserProfileDto>>;

public class GetUserProfilesQueryHandler : IRequestHandler<GetUserProfilesQuery, List<UserProfileDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetUserProfilesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<UserProfileDto>> Handle(GetUserProfilesQuery request, CancellationToken cancellationToken)
    {
        var profiles = await _context.UserProfiles
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<UserProfileDto>>(profiles);
    }
}