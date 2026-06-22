using AuthModule.Core.Interfaces;
using ErrorHandling;
using ErrorHandling.Service;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace AuthModule.Core.Features;

/// <summary>
/// Represents a MediatR request to set a user's status by identifier.
/// </summary>
/// <remarks>Contains required properties UserId and Status. Intended to be handled by a request handler that
/// returns a Result<Unit>.</remarks>
public class SetUserStatusCommand : IRequest<Result<Unit>>
{
    [Required] public Guid UserId { get; set; } = Guid.Empty;
    [Required] public bool Status { get; set; }
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
            User? user = await userRepository.GetUserAsync(x => x.Id == command.UserId, false, ct);

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
