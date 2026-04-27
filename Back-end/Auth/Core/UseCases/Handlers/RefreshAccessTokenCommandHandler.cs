using Core.Domain;
using Core.Interfaces;
using Core.UseCases.Commands;
using DTOs;
using ErrorHandling;
using ErrorHandling.Service;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Models;

namespace Core.UseCases.Handlers
{
    public class RefreshAccessTokenCommandHandler(IIdentityService identityService,
        IRefreshTokenRepository tokenRepository,
        ILogger<RefreshAccessTokenCommandHandler> logger,
        UserManager<User> userManager,
        ErrorFactory errorFactory)
    : IRequestHandler<RefreshAccessTokenCommand, Result<LoginResponse>>
    {
        public async Task<Result<LoginResponse>> Handle(RefreshAccessTokenCommand command, CancellationToken ct)
        {
            try
            {
                RefreshToken? tokenInfo = await tokenRepository.GetRefreshTokenInformationAsync(command.UserId, ct);

                if (tokenInfo == null)
                {
                    return Result<LoginResponse>.Failure(errorFactory.Create(new InvalidOrNonExistingToken()));
                }

                if (tokenInfo.Token != command.RefreshToken || tokenInfo.UserId != command.UserId)
                {
                    return Result<LoginResponse>.Failure(errorFactory.Create(new InvalidOrNonExistingToken()));
                }

                // If someone tries to reuse the same revoked token in a short amount of time, 
                // it could mean a malicious actor is trying to use a replay attack.
                if (tokenInfo.IsRevoked)
                {
                    logger.LogWarning("Credentials might be compromised! User tried to use revoked token with ID {tokenId}", tokenInfo.Id);
                    return Result<LoginResponse>.Failure(errorFactory.Create(new InvalidOrNonExistingToken()));
                }

                ct.ThrowIfCancellationRequested();

                await tokenRepository.RevokeSpecificRefreshTokenAsync(command.UserId, command.RefreshToken, ct);

                User user = await userManager.FindByIdAsync(command.UserId.ToString());

                string accessToken = await identityService.GenerateAccessTokenAsync(user);

                string refreshToken = await identityService.GenerateRefreshTokenAsync(user, ct);

                LoginResponse response = new LoginResponse { AccessToken = accessToken, RefreshToken = refreshToken };

                return Result<LoginResponse>.Success(response);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Operation canceled for user id {userId}", command.UserId);

                return Result<LoginResponse>.Failure(errorFactory.Create(new OperationCanceled()));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);

                return Result<LoginResponse>.Failure(errorFactory.Create(new InternalError()));
            }
        }
    }
}
