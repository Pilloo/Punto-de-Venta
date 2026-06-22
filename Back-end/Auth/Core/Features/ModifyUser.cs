using AuthModule.Core.Interfaces;
using DTOs.Auth;
using ErrorHandling;
using ErrorHandling.Service;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Models;
using System.ComponentModel.DataAnnotations;

namespace AuthModule.Core.Features;

/// <summary>
/// Represents a request to update a user's profile information and optionally change the user's password; returns an
/// authentication token on success.
/// </summary>
/// <remarks>UserId is required. If NewPassword is provided, OldPassword must also be supplied to perform a
/// password change. Properties left empty are treated as unchanged. The command is handled by a handler that returns a
/// Result<TokenDto>.</remarks>
public class ModifyUserCommand : IRequest<Result<TokenDto>>
{
    [Required] public Guid UserId { get; set; } = Guid.Empty;

    public string GivenName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    public string OldPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;
}

public class ModifyUserCommandHandler(
    UserManager<User> userManager,
    ErrorFactory errorFactory,
    IIdentityService identityService,
    ILogger<ModifyUserCommandHandler> logger,
    IRefreshTokenRepository tokenRepository
) : IRequestHandler<ModifyUserCommand, Result<TokenDto>>
{
    public async Task<Result<TokenDto>> Handle(ModifyUserCommand command, CancellationToken ct)
    {
        try
        {
            User? user = await userManager.FindByIdAsync(command.UserId.ToString());

            if (user == null)
            {
                return Result<TokenDto>.Failure(errorFactory.Create(new UserNotFound()));
            }

            bool profileModified = false;

            if (!string.IsNullOrEmpty(command.GivenName))
            {
                if (user.GivenName != command.GivenName)
                {
                    user.GivenName = command.GivenName;
                    profileModified = true;
                }
            }

            if (!string.IsNullOrEmpty(command.LastName))
            {

                if (user.LastName != command.LastName)
                {
                    user.LastName = command.LastName;
                    profileModified = true;
                }
            }

            if (!string.IsNullOrEmpty(command.UserName))
            {
                if (user.UserName != command.UserName)
                {
                    user.UserName = command.UserName;
                    profileModified = true;
                }
            }

            if (profileModified)
            {
                ct.ThrowIfCancellationRequested();

                IdentityResult operationResult = await userManager.UpdateAsync(user);

                if (!operationResult.Succeeded)
                {
                    return Result<TokenDto>.Failure(errorFactory.Create(new InternalError()));
                }
            }

            if (!string.IsNullOrEmpty(command.NewPassword))
            {
                ct.ThrowIfCancellationRequested();

                IdentityResult operationResult = await userManager.ChangePasswordAsync(user, command.OldPassword, command.NewPassword);

                if (!operationResult.Succeeded)
                {
                    return Result<TokenDto>.Failure(errorFactory.Create(new InternalError()));
                }
            }

            await tokenRepository.RevokeAllRefreshTokenAsync(command.UserId, ct);

            string accessToken = await identityService.GenerateAccessTokenAsync(user);
            string refreshToken = await identityService.GenerateRefreshTokenAsync(user, ct);

            return Result<TokenDto>.Success(new TokenDto { AccessToken = accessToken, RefreshToken = refreshToken });
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Operation canceled for user {userId}", command.UserId);

            return Result<TokenDto>.Failure(errorFactory.Create(new InternalError()));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);

            return Result<TokenDto>.Failure(errorFactory.Create(new InternalError()));
        }
    }
}
