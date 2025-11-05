using MigratingAssistant.Domain.Entities;
using MigratingAssistant.Domain.Enums;

namespace MigratingAssistant.Application.Users.Queries.GetUsers;

public class UserBriefDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool EmailVerified { get; set; }
    
    private class Mapping : AutoMapper.Profile
    {
        public Mapping()
        {
            CreateMap<User, UserBriefDto>();
        }
    }
}