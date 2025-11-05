using MigratingAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MigratingAssistant.Application.Bookings.Queries;

public record GetBookingsQuery : IRequest<List<BookingDto>>;

public class GetBookingsQueryHandler : IRequestHandler<GetBookingsQuery, List<BookingDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetBookingsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<BookingDto>> Handle(GetBookingsQuery request, CancellationToken cancellationToken)
    {
        var bookings = await _context.Bookings
            .Include(b => b.Payment)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<BookingDto>>(bookings);
    }
}