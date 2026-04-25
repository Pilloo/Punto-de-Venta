using Core.UseCases.Commands;
using Models;
using ErrorHandling;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Core.Interfaces;
using DTOs;
using ErrorHandling.Service;
using Core.Domain;

namespace Core.UseCases.Handlers
{
    public class LoginHandler(
        UserManager<User> userManager,
        IIdentityService identityService,
        SignInManager<User> signInManager,
        ErrorFactory errorFactory,
        IRefreshTokenRepository refreshTokenRepository) : IRequestHandler<LoginCommand, Result<LoginResponse>>
    {
        public async Task<Result<LoginResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
        {
            User? user = await userManager.FindByNameAsync(command.Username);

            if (user == null)
            {
                return Result<LoginResponse>.Failure(errorFactory.CreateProblemDetails(new InvalidCredentials()));
            }

            SignInResult checkResult = await signInManager.CheckPasswordSignInAsync(user, command.Password, false);

            if (!checkResult.Succeeded)
            {
                return Result<LoginResponse>.Failure(errorFactory.CreateProblemDetails(new InvalidCredentials()));
            }

            string? refreshToken = null;

            if (command.RememberMe)
            {
                refreshToken = await identityService.GenerateRefreshTokenAsync();

                RefreshToken tokenEntity = new RefreshToken
                {
                    UserId = user.Id,
                    Token = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    IsRevoked = false,
                    CreatedAt = DateTime.UtcNow,
                };

                await refreshTokenRepository.SaveRefreshTokenAsync(tokenEntity, cancellationToken);               
            }

            string token = await identityService.GenerateAccessTokenAsync(user);

            LoginResponse response = new LoginResponse
            {
                AccessToken = token,
                RefreshToken = refreshToken
            };

            return Result<LoginResponse>.Success(response);
        }
    }
}
