using MigratingAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MigratingAssistant.Application.SupportTickets.Queries;

public record GetSupportTicketByIdQuery(Guid Id) : IRequest<SupportTicketDto?>;

public class GetSupportTicketByIdQueryHandler : IRequestHandler<GetSupportTicketByIdQuery, SupportTicketDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetSupportTicketByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<SupportTicketDto?> Handle(GetSupportTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _context.SupportTickets
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        return ticket is null ? null : _mapper.Map<SupportTicketDto>(ticket);
    }
}