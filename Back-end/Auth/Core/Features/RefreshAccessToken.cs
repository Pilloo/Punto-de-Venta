using AuthModule.Core.Domain;
using AuthModule.Core.Interfaces;
using DTOs;
using ErrorHandling;
using ErrorHandling.Service;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Models;
using System.ComponentModel.DataAnnotations;

namespace AuthModule.Core.Features;

public class RefreshAccessTokenCommand : IRequest<Result<TokenDto>>
{
    [Required] public Guid UserId { get; set; } = Guid.Empty;
    [Required] public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshAccessTokenCommandHandler(
    IIdentityService identityService,
    IRefreshTokenRepository tokenRepository,
    ILogger<RefreshAccessTokenCommandHandler> logger,
    UserManager<User> userManager,
    ErrorFactory errorFactory
) : IRequestHandler<RefreshAccessTokenCommand, Result<TokenDto>>
{
    public async Task<Result<TokenDto>> Handle(RefreshAccessTokenCommand command, CancellationToken ct)
    {
        try
        {
            User? user = await userManager.FindByIdAsync(command.UserId.ToString());

            if (user == null)
            {
                return Result<TokenDto>.Failure(errorFactory.Create(new UserNotFound()));
            }

            RefreshToken? tokenInfo = await tokenRepository.GetRefreshTokenInformationAsync(command.UserId, ct);

            if (tokenInfo == null)
            {
                return Result<TokenDto>.Failure(errorFactory.Create(new InvalidOrNonExistingToken()));
            }

            if (tokenInfo.Token != command.RefreshToken || tokenInfo.UserId != command.UserId)
            {
                return Result<TokenDto>.Failure(errorFactory.Create(new InvalidOrNonExistingToken()));
            }

            // If someone tries to reuse the same revoked token in a short amount of time, 
            // it could mean a malicious actor is trying to use a replay attack.
            if (tokenInfo.IsRevoked)
            {
                logger.LogWarning("Credentials might be compromised! User tried to use revoked token with ID {tokenId}", tokenInfo.Id);
                return Result<TokenDto>.Failure(errorFactory.Create(new InvalidOrNonExistingToken()));
            }

            ct.ThrowIfCancellationRequested();

            await tokenRepository.RevokeSpecificRefreshTokenAsync(command.UserId, command.RefreshToken, ct);

            string accessToken = await identityService.GenerateAccessTokenAsync(user);

            string refreshToken = await identityService.GenerateRefreshTokenAsync(user, ct);

            TokenDto response = new TokenDto { AccessToken = accessToken, RefreshToken = refreshToken };

            return Result<TokenDto>.Success(response);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Operation canceled for user id {userId}", command.UserId);

            return Result<TokenDto>.Failure(errorFactory.Create(new OperationCanceled()));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);

            return Result<TokenDto>.Failure(errorFactory.Create(new InternalError()));
        }
    }
}
