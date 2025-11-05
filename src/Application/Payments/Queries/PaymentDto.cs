using MigratingAssistant.Domain.Entities;

namespace MigratingAssistant.Application.Payments.Queries;

public class PaymentDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string? Status { get; set; }
    public string? PaymentMethod { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? TransactionId { get; set; }
    public Guid UserId { get; set; }
    public Guid BookingId { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Payment, PaymentDto>();
        }
    }
}