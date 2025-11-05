using MigratingAssistant.Domain.Entities;

namespace MigratingAssistant.Application.Users.Queries;

public class UserDto
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public UserProfile? Profile { get; set; }
    public ServiceProvider? ServiceProvider { get; set; }
    public IList<Document> Documents { get; set; } = new List<Document>();
    public IList<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();
    public IList<Booking> Bookings { get; set; } = new List<Booking>();
    public IList<JobApplication> JobApplications { get; set; } = new List<JobApplication>();

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<User, UserDto>();
        }
    }
}