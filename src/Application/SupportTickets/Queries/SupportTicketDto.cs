using MigratingAssistant.Domain.Entities;

namespace MigratingAssistant.Application.SupportTickets.Queries;

public class SupportTicketDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public int Status { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<SupportTicket, SupportTicketDto>()
                .ForMember(d => d.Status, opt => opt.MapFrom(s => (int)s.Status));
        }
    }
}