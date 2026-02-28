using MeetNest.Domain.Entities;

namespace MeetNest.Application.Interfaces.Security;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user);

    string GenerateRefreshToken();
}

