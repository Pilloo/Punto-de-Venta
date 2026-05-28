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

public class LoginCommand : IRequest<Result<TokenDto>>
{
    [Required] public string Username { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
    [Required] public bool RememberMe { get; set; } = false;
}

public class LoginCommandHandler(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ErrorFactory errorFactory,
    ILogger<LoginCommandHandler> logger,
    IIdentityService identityService
) : IRequestHandler<LoginCommand, Result<TokenDto>>
{
    public async Task<Result<TokenDto>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        try
        {
            User? user = await userManager.FindByNameAsync(command.Username);

            if (user == null)
            {
                return Result<TokenDto>.Failure(errorFactory.Create(new InvalidCredentials()));
            }

            SignInResult checkResult = await signInManager.CheckPasswordSignInAsync(user, command.Password, false);

            if (!checkResult.Succeeded)
            {
                return Result<TokenDto>.Failure(errorFactory.Create(new InvalidCredentials()));
            }

            cancellationToken.ThrowIfCancellationRequested();

            string? refreshToken = null;

            if (command.RememberMe)
            {
                refreshToken = await identityService.GenerateRefreshTokenAsync(user, cancellationToken);
            }

            string token = await identityService.GenerateAccessTokenAsync(user);

            TokenDto response = new()
            {
                AccessToken = token,
                RefreshToken = refreshToken
            };

            return Result<TokenDto>.Success(response);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Login operation canceled for user {Username}", command.Username);

            return Result<TokenDto>.Failure(errorFactory.Create(new OperationCanceled()));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);

            return Result<TokenDto>.Failure(errorFactory.Create(new InternalError()));
        }
    }
}
