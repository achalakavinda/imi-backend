using MigratingAssistant.Domain.Entities;

namespace MigratingAssistant.Application.Bookings.Queries;

public class BookingDto
{
    public Guid Id { get; set; }
    public DateTime BookingDateTime { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
    public decimal? Duration { get; set; }
    public Guid UserId { get; set; }
    public Guid ServiceProviderId { get; set; }
    public Payment? Payment { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Booking, BookingDto>();
        }
    }
}