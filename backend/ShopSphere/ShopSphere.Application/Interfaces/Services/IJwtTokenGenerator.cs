using ShopSphere.Domain.Entities;

namespace ShopSphere.Application.Interfaces.Services;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(User user);
    RefreshToken GenerateRefreshToken(Guid userId);
}
