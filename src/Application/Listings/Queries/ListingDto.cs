namespace MigratingAssistant.Application.Listings.Queries;

public class ListingDto
{
    public Guid Id { get; init; }
    public int ServiceTypeId { get; init; }
    public Guid ProviderId { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? Attributes { get; init; }
    public decimal? Price { get; init; }
    public string? Currency { get; init; }
    public int Status { get; init; }
    public DateTimeOffset? AvailableFrom { get; init; }
    public DateTimeOffset? AvailableTo { get; init; }
    public int Version { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Listing, ListingDto>()
                .ForMember(d => d.Status, opt => opt.MapFrom(s => (int)s.Status));
        }
    }
}
