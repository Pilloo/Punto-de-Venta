using Core.UseCases.Commands;
using Models;
using ErrorHandling;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Core.Interfaces;
using DTOs;
using ErrorHandling.Service;
using Core.Domain;
using Microsoft.Extensions.Logging;

namespace Core.UseCases.Handlers
{
    public class LoginCommandHandler(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ErrorFactory errorFactory,
        ILogger<LoginCommandHandler> logger,
        IIdentityService identityService) : IRequestHandler<LoginCommand, Result<LoginResponse>>
    {
        public async Task<Result<LoginResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
        {
            try
            {
                User? user = await userManager.FindByNameAsync(command.Username);

                if (user == null)
                {
                    return Result<LoginResponse>.Failure(errorFactory.Create(new InvalidCredentials()));
                }
                
                SignInResult checkResult = await signInManager.CheckPasswordSignInAsync(user, command.Password, false);

                if (!checkResult.Succeeded)
                {
                    return Result<LoginResponse>.Failure(errorFactory.Create(new InvalidCredentials()));
                }

                cancellationToken.ThrowIfCancellationRequested();

                string? refreshToken = null;

                if (command.RememberMe)
                {
                    refreshToken = await identityService.GenerateRefreshTokenAsync(user, cancellationToken);
                }

                string token = await identityService.GenerateAccessTokenAsync(user);

                LoginResponse response = new LoginResponse
                {
                    AccessToken = token,
                    RefreshToken = refreshToken
                };

                return Result<LoginResponse>.Success(response);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Login operation canceled for user {Username}", command.Username);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);

                return Result<LoginResponse>.Failure(errorFactory.Create(new InternalError()));
            }
        }
    }
}
