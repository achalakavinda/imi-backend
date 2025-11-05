using MigratingAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MigratingAssistant.Application.Payments.Queries;

public record GetPaymentsQuery : IRequest<List<PaymentDto>>;

public class GetPaymentsQueryHandler : IRequestHandler<GetPaymentsQuery, List<PaymentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPaymentsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<PaymentDto>> Handle(GetPaymentsQuery request, CancellationToken cancellationToken)
    {
        var payments = await _context.Payments
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<PaymentDto>>(payments);
    }
}