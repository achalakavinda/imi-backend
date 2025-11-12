using MigratingAssistant.Domain.Entities;

namespace MigratingAssistant.Application.ServiceProviders.Queries;

public class ServiceProviderDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? ProviderName { get; set; }
    public string? ProviderType { get; set; }
    public bool Verified { get; set; }
    public string? ProviderMetadata { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ServiceProvider, ServiceProviderDto>();
        }
    }
}