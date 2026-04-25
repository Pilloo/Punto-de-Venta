using Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task SaveRefreshTokenAsync(RefreshToken tokens, CancellationToken ct);
        Task<RefreshToken?> GetRefreshTokenInformationAsync(Guid userId, CancellationToken ct);
        Task RevokeAllRefreshTokenAsync(Guid userId, CancellationToken ct);
        Task RevokeSpecificRefreshTokenAsync(Guid userId, string refreshToken, CancellationToken ct);
    }
}
