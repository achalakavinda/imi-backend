using MigratingAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MigratingAssistant.Application.Documents.Queries;

public record GetDocumentsQuery : IRequest<List<DocumentDto>>;

public class GetDocumentsQueryHandler : IRequestHandler<GetDocumentsQuery, List<DocumentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDocumentsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<DocumentDto>> Handle(GetDocumentsQuery request, CancellationToken cancellationToken)
    {
        var documents = await _context.Documents
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<DocumentDto>>(documents);
    }
}