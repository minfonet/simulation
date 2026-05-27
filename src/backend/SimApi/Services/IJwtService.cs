namespace SimApi.Services;

public interface IJwtService
{
    string GenerateAccessToken(Guid userId, string email, string role, Guid organizationId);
    string GenerateRefreshToken();
}
