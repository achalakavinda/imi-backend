using MigratingAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MigratingAssistant.Application.SupportTickets.Queries;

public record GetSupportTicketsQuery : IRequest<List<SupportTicketDto>>;

public class GetSupportTicketsQueryHandler : IRequestHandler<GetSupportTicketsQuery, List<SupportTicketDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetSupportTicketsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<SupportTicketDto>> Handle(GetSupportTicketsQuery request, CancellationToken cancellationToken)
    {
        var tickets = await _context.SupportTickets
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<SupportTicketDto>>(tickets);
    }
}