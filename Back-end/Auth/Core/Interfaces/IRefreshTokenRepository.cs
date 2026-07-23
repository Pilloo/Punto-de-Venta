using AuthModule.Core.Domain;

namespace AuthModule.Core.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task SaveRefreshTokenAsync(RefreshToken tokens, CancellationToken ct);
        Task<RefreshToken?> GetRefreshTokenInformationAsync(Guid userId, CancellationToken ct);
        Task RevokeAllRefreshTokenAsync(Guid userId, CancellationToken ct);
        Task RevokeSpecificRefreshTokenAsync(Guid userId, string refreshToken, CancellationToken ct);
    }
}
