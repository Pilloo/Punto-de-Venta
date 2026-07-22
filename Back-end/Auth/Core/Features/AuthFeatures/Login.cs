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
/// Represents a command used for user authentication.
/// </summary>
/// <remarks>
/// This command is utilized to authenticate a user by validating their credentials
/// and optionally determining if the session should be persisted via the "RememberMe" property.
/// </remarks>
/// <example>
/// This command is typically sent to a handler that processes user login functionality.
/// </example>
public record LoginCommand : IRequest<Result<TokenResponse>>
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public bool RememberMe { get; init; } = false;

    public static LoginCommand FromDto(LoginRequest request) => new()
    {
        Username = request.Username,
        Password = request.Password,
        RememberMe = request.RememberMe
    };
}

public class LoginCommandHandler(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ErrorFactory errorFactory,
    ILogger<LoginCommandHandler> logger,
    IIdentityService identityService
) : IRequestHandler<LoginCommand, Result<TokenResponse>>
{
    public async Task<Result<TokenResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        try
        {
            User? user = await userManager.FindByNameAsync(command.Username);

            if (user == null)
            {
                return Result<TokenResponse>.Failure(errorFactory.Create(new InvalidCredentials()));
            }

            SignInResult checkResult = await signInManager.CheckPasswordSignInAsync(user, command.Password, false);

            if (!checkResult.Succeeded)
            {
                return Result<TokenResponse>.Failure(errorFactory.Create(new InvalidCredentials()));
            }

            cancellationToken.ThrowIfCancellationRequested();

            string? refreshToken = null;

            if (command.RememberMe)
            {
                refreshToken = await identityService.GenerateRefreshTokenAsync(user, cancellationToken);
            }

            string token = await identityService.GenerateAccessTokenAsync(user);

            TokenResponse response = new()
            {
                AccessToken = token,
                RefreshToken = refreshToken
            };

            return Result<TokenResponse>.Success(response);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Login operation canceled for user {Username}", command.Username);

            return Result<TokenResponse>.Failure(errorFactory.Create(new OperationCanceled()));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);

            return Result<TokenResponse>.Failure(errorFactory.Create(new InternalError()));
        }
    }
}
