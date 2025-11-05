using MigratingAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MigratingAssistant.Application.Documents.Queries;

public record GetDocumentByIdQuery(Guid Id) : IRequest<DocumentDto?>;

public class GetDocumentByIdQueryHandler : IRequestHandler<GetDocumentByIdQuery, DocumentDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDocumentByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<DocumentDto?> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        var document = await _context.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        return document is null ? null : _mapper.Map<DocumentDto>(document);
    }
}