using MigratingAssistant.Domain.Entities;

namespace MigratingAssistant.Application.Common.Models;

public class LookupDto
{
    public Guid Id { get; init; }

    public string? Title { get; init; }

    private class Mapping : AutoMapper.Profile
    {
        public Mapping()
        {
            // Add mappings for new entities here
            // CreateMap<Entity, LookupDto>();
        }
    }
}
