using Core.Interfaces;
using Core.UseCases.Commands;
using ErrorHandling;
using ErrorHandling.Service;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.UseCases.Handlers
{
    public class DeleteUserCommandHandler(IUserStore<User> userStore,
        ErrorFactory errorFactory,
        ILogger logger,
        IRefreshTokenRepository tokenRepository)
        : IRequestHandler<DeleteUserCommand, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(DeleteUserCommand command, CancellationToken ct)
        {
            try
            {
                User? user = await userStore.FindByIdAsync(command.UserId.ToString(), ct);

                if (user == null)
                {
                    return Result<Unit>.Failure(errorFactory.Create(new UserNotFound()));
                }

                ct.ThrowIfCancellationRequested();

                user.IsActive = false;
                
                await userStore.UpdateAsync(user, ct);

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
}
