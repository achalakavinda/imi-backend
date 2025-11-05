using MigratingAssistant.Domain.Entities;

namespace MigratingAssistant.Application.ServiceProviders.Queries;

public class ServiceProviderDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? CompanyName { get; set; }
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? ContactEmail { get; set; }
    public IList<Booking> Bookings { get; set; } = new List<Booking>();

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ServiceProvider, ServiceProviderDto>();
        }
    }
}