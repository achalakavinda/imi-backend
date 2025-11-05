using MigratingAssistant.Domain.Entities;

namespace MigratingAssistant.Application.SupportTickets.Queries;

public class SupportTicketDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }


    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<SupportTicket, SupportTicketDto>();
        }
    }
}