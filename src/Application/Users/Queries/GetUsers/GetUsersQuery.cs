namespace MigratingAssistant.Application.Users.Queries.GetUsers;

public record GetUsersQuery : IRequest<IList<UserBriefDto>>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, IList<UserBriefDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetUsersQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IList<UserBriefDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        return await _context.Users
            .ProjectTo<UserBriefDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}