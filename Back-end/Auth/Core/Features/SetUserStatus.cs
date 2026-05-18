using AuthModule.Core.Interfaces;
using ErrorHandling;
using ErrorHandling.Service;
using MediatR;
using Microsoft.Extensions.Logging;
using Models;
using System.ComponentModel.DataAnnotations;

namespace AuthModule.Core.Features;

public class SetUserStatusCommand : IRequest<Result<Unit>>
{
    [Required] public Guid UserId { get; set; } = Guid.Empty;
    [Required] public bool Status { get; set; }
}

public class SetUserStatusCommandHandler(
    IUserRepository userRepository,
    ErrorFactory errorFactory,
    ILogger<SetUserStatusCommand> logger,
    IRefreshTokenRepository tokenRepository
) : IRequestHandler<SetUserStatusCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(SetUserStatusCommand command, CancellationToken ct)
    {
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
