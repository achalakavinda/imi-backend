using MigratingAssistant.Domain.Entities;

namespace MigratingAssistant.Application.Documents.Queries;

public class DocumentDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? DocType { get; set; }
    public string? StoragePath { get; set; }
    public bool Verified { get; set; }
    public DateTimeOffset UploadedAt { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Document, DocumentDto>();
        }
    }
}