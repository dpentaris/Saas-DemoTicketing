namespace CareLog.Application.Abstractions;

public interface IJwtTokenService
{
    string CreateAccessToken(string userId, Guid tenantId, IEnumerable<string> roles);
    string CreateRefreshToken();
}
