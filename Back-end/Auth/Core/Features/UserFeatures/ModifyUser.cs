using AuthModule.Core.Interfaces;
using DTOs.Users;
using ErrorHandling;
using ErrorHandling.Service;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Models;
using System.ComponentModel.DataAnnotations;

namespace AuthModule.Core.Features;

/// <summary>
/// Represents a command to modify an existing user's information in the system.
/// This command is processed through the MediatR pipeline and may trigger
/// validation and downstream processing behaviors before execution.
/// </summary>
/// <remarks>
/// The command contains properties for updating various aspects of the user,
/// including name, username, and password. Password changes involve an existing password
/// validation before applying the new password.
/// </remarks>
/// <returns>
/// A <see cref="Result{T}"/> object wrapping a <see cref="TokenResponse"/>.
/// This indicates the success or failure of the operation and may include
/// associated tokens upon success.
/// </returns>
public record ModifyUserCommand : IRequest<Result<Unit>>
{
    public Guid UserId { get; set; } = Guid.Empty;

    public string GivenName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string OldPassword { get; set; } = string.Empty;
    
    public string NewPassword { get; set; } = string.Empty;
    
    public static ModifyUserCommand FromDto(ModifyUserRequest request) => new()
    {
        GivenName = request.GivenName,
        LastName = request.LastName,
        NewPassword = request.NewPassword,
        OldPassword = request.OldPassword,
        UserId = request.UserId,
        UserName = request.UserName
    };
}

public class ModifyUserCommandHandler(
    UserManager<User> userManager,
    ErrorFactory errorFactory,
    IIdentityService identityService,
    ILogger<ModifyUserCommandHandler> logger,
    IRefreshTokenRepository tokenRepository
) : IRequestHandler<ModifyUserCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(ModifyUserCommand command, CancellationToken ct)
    {
        try
        {
            User? user = await userManager.FindByIdAsync(command.UserId.ToString());

            if (user == null)
            {
                return Result<Unit>.Failure(errorFactory.Create(new UserNotFound()));
            }

            bool profileModified = false;
            bool shouldRevokeRefreshTokens = false;

            if (!string.IsNullOrWhiteSpace(command.GivenName))
            {
                if (user.GivenName != command.GivenName)
                {
                    user.GivenName = command.GivenName;
                    profileModified = true;
                }
            }

            if (!string.IsNullOrWhiteSpace(command.LastName))
            {

                if (user.LastName != command.LastName)
                {
                    user.LastName = command.LastName;
                    profileModified = true;
                }
            }

            if (!string.IsNullOrWhiteSpace(command.UserName))
            {
                if (user.UserName != command.UserName)
                {
                    user.UserName = command.UserName;
                    profileModified = true;
                    shouldRevokeRefreshTokens = true;
                }
            }

            if (profileModified)
            {
                ct.ThrowIfCancellationRequested();

                IdentityResult operationResult = await userManager.UpdateAsync(user);

                if (!operationResult.Succeeded)
                {
                    return Result<Unit>.Failure(errorFactory.Create(new ValidationFailed(operationResult.Errors)));
                }
            }

            if (!string.IsNullOrEmpty(command.NewPassword))
            {
                ct.ThrowIfCancellationRequested();

                IdentityResult operationResult = await userManager.ChangePasswordAsync(user, command.OldPassword, command.NewPassword);

                if (!operationResult.Succeeded)
                {
                    return Result<Unit>.Failure(errorFactory.Create(new InternalError()));
                }
                
                shouldRevokeRefreshTokens = true;
            }

            if (shouldRevokeRefreshTokens)
            {
                await tokenRepository.RevokeAllRefreshTokenAsync(command.UserId, ct);
            }

            return Result<Unit>.Success(Unit.Value);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Operation canceled for user {userId}", command.UserId);

            return Result<Unit>.Failure(errorFactory.Create(new InternalError()));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);

            return Result<Unit>.Failure(errorFactory.Create(new InternalError()));
        }
    }
}
