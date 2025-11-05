using MigratingAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MigratingAssistant.Application.Payments.Queries;

public record GetPaymentByIdQuery(Guid Id) : IRequest<PaymentDto?>;

public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, PaymentDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPaymentByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaymentDto?> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        return payment is null ? null : _mapper.Map<PaymentDto>(payment);
    }
}