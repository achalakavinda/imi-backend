using MigratingAssistant.Domain.Entities;

namespace MigratingAssistant.Application.Documents.Queries;

public class DocumentDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? FilePath { get; set; }
    public string? FileType { get; set; }
    public DateTime UploadDate { get; set; }
    public Guid UserId { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Document, DocumentDto>();
        }
    }
}