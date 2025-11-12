using MigratingAssistant.Domain.Entities;

namespace MigratingAssistant.Application.Payments.Queries;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? GatewayReference { get; set; }
    public int Status { get; set; }
    public string? Meta { get; set; }
    public string? IdempotencyKey { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Payment, PaymentDto>()
                .ForMember(d => d.Status, opt => opt.MapFrom(s => (int)s.Status));
        }
    }
}