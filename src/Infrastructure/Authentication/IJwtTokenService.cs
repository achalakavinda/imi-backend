using System.Security.Claims;

namespace MigratingAssistant.Infrastructure.Authentication;

public interface IJwtTokenService
{
    string GenerateAccessToken(string userId, string email, IEnumerable<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
