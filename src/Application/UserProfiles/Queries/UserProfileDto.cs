using MigratingAssistant.Domain.Entities;

namespace MigratingAssistant.Application.UserProfiles.Queries;

public class UserProfileDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Nationality { get; set; }
    public string? Bio { get; set; }
    public string? Preferences { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<UserProfile, UserProfileDto>();
        }
    }
}