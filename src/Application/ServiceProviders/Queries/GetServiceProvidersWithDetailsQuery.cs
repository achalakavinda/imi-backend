using MigratingAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MigratingAssistant.Application.ServiceProviders.Queries;

public record GetServiceProvidersWithDetailsQuery : IRequest<List<ServiceProviderDto>>;

public class GetServiceProvidersWithDetailsQueryHandler : IRequestHandler<GetServiceProvidersWithDetailsQuery, List<ServiceProviderDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetServiceProvidersWithDetailsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ServiceProviderDto>> Handle(GetServiceProvidersWithDetailsQuery request, CancellationToken cancellationToken)
    {
        var serviceProviders = await _context.ServiceProviders
            .Include(sp => sp.Listings)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<ServiceProviderDto>>(serviceProviders);
    }
}