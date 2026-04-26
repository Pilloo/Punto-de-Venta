using Models;

namespace Core.Interfaces;

public interface IIdentityService
{
    Task<String> GenerateAccessTokenAsync(User user);
    Task<String> GenerateRefreshTokenAsync(User user, CancellationToken ct);
}