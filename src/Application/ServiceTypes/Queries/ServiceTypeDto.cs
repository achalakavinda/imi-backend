namespace MigratingAssistant.Application.ServiceTypes.Queries;

public class ServiceTypeDto
{
    public int Id { get; init; }
    public string ServiceKey { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string? SchemaHint { get; init; }
    public bool Enabled { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ServiceType, ServiceTypeDto>();
        }
    }
}
