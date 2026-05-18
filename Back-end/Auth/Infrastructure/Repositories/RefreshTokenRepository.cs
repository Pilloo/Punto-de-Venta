using AuthModule.Core.Interfaces;
using AuthModule.Core.Domain;
using AuthModule.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuthModule.Infrastructure.Repositiories
{
    public class RefreshTokenRepository(AuthDbContext context) : IRefreshTokenRepository
    {
        /// <summary>
        /// Asynchronously saves the specified refresh token to the data store.
        /// </summary>
        /// <param name="token">The refresh token to persist. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        public async Task SaveRefreshTokenAsync(RefreshToken token, CancellationToken ct)
        {
            try
            {
                context.RefreshTokens.Add(token);

                await context.SaveChangesAsync(ct);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves the most recent, non-revoked refresh token for the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose refresh token information is to be retrieved.</param>
        /// <param name="ct">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the most recent, non-revoked
        /// <see cref="RefreshToken"/> for the specified user, or <see langword="null"/> if no such token exists.</returns>
        public async Task<RefreshToken?> GetRefreshTokenInformationAsync(Guid userId, CancellationToken ct)
        {
            try
            {
                RefreshToken? operationResult = await context.RefreshTokens.Where(x => x.IsRevoked == false)
                                                                           .Where(x => x.UserId == userId)
                                                                           .OrderByDescending(x => x.CreatedAt)
                                                                           .AsNoTracking()
                                                                           .FirstOrDefaultAsync(ct);

                return operationResult;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Revokes all active refresh tokens associated with the specified user.
        /// </summary>
        /// <remarks>This method marks all non-revoked refresh tokens for the specified user as revoked in
        /// the database. The operation is performed directly in the data store and does not load entities into
        /// memory.</remarks>
        /// <param name="userId">The unique identifier of the user whose refresh tokens will be revoked.</param>
        /// <param name="ct">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task RevokeAllRefreshTokenAsync(Guid userId, CancellationToken ct)
        {
            try
            {
                IQueryable<RefreshToken> refreshTokens = context.RefreshTokens.Where(x => x.UserId == userId && !x.IsRevoked);

                // Executes the action directly on the DB.
                await refreshTokens.ExecuteUpdateAsync(s => s.SetProperty(t => t.IsRevoked, true), ct);

                return;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Revokes a specific refresh token for the specified user, preventing further use of that token.
        /// </summary>
        /// <remarks>If the specified refresh token is not found for the user, the method completes
        /// without making changes. This method does not throw an exception if the token is not present.</remarks>
        /// <param name="userId">The unique identifier of the user whose refresh token is to be revoked.</param>
        /// <param name="refreshToken">The refresh token string to revoke. If the token does not exist for the user, no action is taken.</param>
        /// <param name="ct">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task RevokeSpecificRefreshTokenAsync(Guid userId, string refreshToken, CancellationToken ct)
        {
            try
            {
                RefreshToken? token = await context.RefreshTokens.Where(x => x.UserId == userId && x.Token == refreshToken).FirstOrDefaultAsync(ct);

                if (token == null) return;

                token.IsRevoked = true;

                await context.SaveChangesAsync(ct);
            }
            catch
            {
                throw;
            }
        }
    }
}
