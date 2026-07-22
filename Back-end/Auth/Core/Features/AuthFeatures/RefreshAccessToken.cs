using AuthModule.Core.Domain;
using AuthModule.Core.Interfaces;
using DTOs.Auth;
using ErrorHandling;
using ErrorHandling.Service;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Models;

namespace AuthModule.Core.Features.AuthFeatures;

/// <summary>
/// Represents a command to refresh an access token for a given user and refresh token.
/// </summary>
/// <remarks>
/// This command is used to request a new access token and refresh token for a user.
/// It is typically invoked when the current access token is expired or near expiration,
/// and the user provides a valid refresh token to maintain their session.
/// </remarks>
/// <example>
/// The <c>RefreshAccessTokenCommand</c> encapsulates the data required to perform
/// the operation, including the user's unique identifier and the current refresh token.
/// </example>
/// <seealso cref="DTOs.Auth.RefreshAccessTokenRequest"/>
/// <seealso cref="TokenResponse"/>
/// <seealso cref="ErrorHandling.Result{T}"/>
public record RefreshAccessTokenCommand : IRequest<Result<TokenResponse>>
{
    public Guid UserId { get; private init; } = Guid.Empty;
    public string RefreshToken { get; private init; } = string.Empty;

    public static RefreshAccessTokenCommand FromDto(RefreshAccessTokenRequest request) => new()
    {
        UserId = request.UserId,
        RefreshToken = request.RefreshToken
    };
}

public class RefreshAccessTokenCommandHandler(
    IIdentityService identityService,
    IRefreshTokenRepository tokenRepository,
    ILogger<RefreshAccessTokenCommandHandler> logger,
    UserManager<User> userManager,
    ErrorFactory errorFactory
) : IRequestHandler<RefreshAccessTokenCommand, Result<TokenResponse>>
{
    public async Task<Result<TokenResponse>> Handle(RefreshAccessTokenCommand command, CancellationToken ct)
    {
        try
        {
            User? user = await userManager.FindByIdAsync(command.UserId.ToString());

            if (user == null)
            {
                return Result<TokenResponse>.Failure(errorFactory.Create(new UserNotFound()));
            }

            RefreshToken? tokenInfo = await tokenRepository.GetRefreshTokenInformationAsync(command.UserId, ct);

            if (tokenInfo == null)
            {
                return Result<TokenResponse>.Failure(errorFactory.Create(new InvalidOrNonExistingToken()));
            }

            if (tokenInfo.Token != command.RefreshToken || tokenInfo.UserId != command.UserId)
            {
                return Result<TokenResponse>.Failure(errorFactory.Create(new InvalidOrNonExistingToken()));
            }

            // If someone tries to reuse the same revoked token in a short amount of time, 
            // it could mean a malicious actor is trying to use a replay attack.
            if (tokenInfo.IsRevoked)
            {
                logger.LogWarning("Credentials might be compromised! User tried to use revoked token with ID {tokenId}",
                                  tokenInfo.Id);
                return Result<TokenResponse>.Failure(errorFactory.Create(new InvalidOrNonExistingToken()));
            }

            ct.ThrowIfCancellationRequested();

            await tokenRepository.RevokeSpecificRefreshTokenAsync(command.UserId, command.RefreshToken, ct);

            string accessToken = await identityService.GenerateAccessTokenAsync(user);

            string refreshToken = await identityService.GenerateRefreshTokenAsync(user, ct);

            TokenResponse response = new() { AccessToken = accessToken, RefreshToken = refreshToken };

            return Result<TokenResponse>.Success(response);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Operation canceled for user id {userId}", command.UserId);

            return Result<TokenResponse>.Failure(errorFactory.Create(new OperationCanceled()));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);

            return Result<TokenResponse>.Failure(errorFactory.Create(new InternalError()));
        }
    }
}