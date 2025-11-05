using MigratingAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MigratingAssistant.Application.ServiceProviders.Queries;

public record GetServiceProviderByIdWithDetailsQuery(Guid Id) : IRequest<ServiceProviderDto?>;

public class GetServiceProviderByIdWithDetailsQueryHandler : IRequestHandler<GetServiceProviderByIdWithDetailsQuery, ServiceProviderDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetServiceProviderByIdWithDetailsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ServiceProviderDto?> Handle(GetServiceProviderByIdWithDetailsQuery request, CancellationToken cancellationToken)
    {
        var serviceProvider = await _context.ServiceProviders
            .Include(sp => sp.Listings)
            .AsNoTracking()
            .FirstOrDefaultAsync(sp => sp.Id == request.Id, cancellationToken);

        return serviceProvider is null ? null : _mapper.Map<ServiceProviderDto>(serviceProvider);
    }
}