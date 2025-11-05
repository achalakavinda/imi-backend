using MigratingAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MigratingAssistant.Application.Bookings.Queries;

public record GetBookingByIdQuery(Guid Id) : IRequest<BookingDto?>;

public class GetBookingByIdQueryHandler : IRequestHandler<GetBookingByIdQuery, BookingDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetBookingByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<BookingDto?> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
    {
        var booking = await _context.Bookings
            .Include(b => b.Payment)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        return booking is null ? null : _mapper.Map<BookingDto>(booking);
    }
}