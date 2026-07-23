using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AuthModule.Core.Interfaces;
using DTOs.Users;
using ErrorHandling;
using ErrorHandling.Service;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Models.Auth;

namespace AuthModule.Core.Features.UserFeatures;

/// <summary>
/// Command to update the status of a user in the system.
/// </summary>
/// <remarks>
/// This command encapsulates the data required to change the active status of a user, identified by their unique identifier.
/// It is used within a CQRS framework to process user status updates via a handler that executes the operation.
/// </remarks>
public class SetUserStatusCommand : IRequest<Result<Unit>>
{
    [Required] public Guid UserId { get; private init; } = Guid.Empty;
    [Required] public bool Status { get; private init; }

    public static SetUserStatusCommand FromDto(SetUserStatusRequest request) => new()
    {
        UserId = request.UserId,
        Status = request.Status,
    };
}

public class SetUserStatusCommandHandler(
    IUserRepository userRepository,
    ErrorFactory errorFactory,
    IHttpContextAccessor httpContext,
    ILogger<SetUserStatusCommand> logger,
    IRefreshTokenRepository tokenRepository
) : IRequestHandler<SetUserStatusCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(SetUserStatusCommand command, CancellationToken ct)
    {
        if (httpContext.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) == command.UserId.ToString())
        {
            return Result<Unit>.Failure(errorFactory.Create(new UserSelfStatusModificationViolation()));
        }

        try
        {
            User? user = await userRepository.GetUserAsync(x => x.Id == command.UserId, false, true, ct);

            if (user == null)
            {
                return Result<Unit>.Failure(errorFactory.Create(new UserNotFound()));
            }

            ct.ThrowIfCancellationRequested();

            user.IsActive = command.Status;

            await userRepository.SaveChangesAsync(ct);

            await tokenRepository.RevokeAllRefreshTokenAsync(user.Id, ct);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Operation canceled for user {userId}", command.UserId);

            return Result<Unit>.Failure(errorFactory.Create(new OperationCanceled()));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);

            return Result<Unit>.Failure(errorFactory.Create(new InternalError()));
        }
    }
}