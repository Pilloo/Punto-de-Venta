using AuthModule.Core.Interfaces;
using ErrorHandling;
using ErrorHandling.Service;
using MediatR;
using Microsoft.Extensions.Logging;
using Models;

namespace AuthModule.Core.Features;

public class GetUserQuery : IRequest<Result<User?>>
{
    public Guid UserId { get; set; }
}

public class GetUserHandler(IUserRepository userRepository, ErrorFactory errorFactory, ILogger<GetUserHandler> logger) : IRequestHandler<GetUserQuery, Result<User?>>
{
    public async Task<Result<User?>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            User? user = await userRepository.GetUserAsync(x => x.Id == request.UserId, true, cancellationToken);

            return Result<User?>.Success(user);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Operation cancelled for user {userId}", request.UserId);

            return Result<User?>.Failure(errorFactory.Create(new OperationCanceled()));
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);

            return Result<User?>.Failure(errorFactory.Create(new InternalError()));
        }
    }
}