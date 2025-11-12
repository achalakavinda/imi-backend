using MigratingAssistant.Domain.Entities;
using MigratingAssistant.Domain.Enums;

namespace MigratingAssistant.Application.Bookings.Queries;

public class BookingDto
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public Guid? InventoryItemId { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset? StartAt { get; set; }
    public DateTimeOffset? EndAt { get; set; }
    public BookingStatus Status { get; set; }
    public Guid? PaymentId { get; set; }
    public Payment? Payment { get; set; }
    public string? IdempotencyKey { get; set; }
    public int Version { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Booking, BookingDto>();
        }
    }
}