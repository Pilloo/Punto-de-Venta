using AuthModule.Core.Interfaces;
using DTOs.Users;
using ErrorHandling;
using ErrorHandling.Service;
using MediatR;
using Microsoft.Extensions.Logging;
using Models;

namespace AuthModule.Core.Features.UserFeatures;

/// <summary>
/// Represents a query to retrieve a user by their unique identifier, with an optional filter
/// to include inactive users in the results.
/// </summary>
/// <remarks>
/// This query is used in the context of a request-response pattern, particularly with MediatR,
/// where it returns a <see cref="Result{T}"/> containing a <see cref="User"/> object or an error.
/// </remarks>
public record GetUserByIdQuery : IRequest<Result<UserResponse>>
{
    public Guid UserId { get; init; }
    public bool IncludeInactiveUsers { get; init; } = false;

    public static GetUserByIdQuery FromQueryParams(Guid userId, bool includeInactiveUsers) => new()
    {
        UserId = userId,
        IncludeInactiveUsers = includeInactiveUsers
    };
}

public class GetUserByIdHandler(
    IUserRepository userRepository,
    ErrorFactory errorFactory,
    ILogger<GetUserByIdHandler> logger) : IRequestHandler<GetUserByIdQuery, Result<UserResponse>>
{
    public async Task<Result<UserResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            User? user = await userRepository.GetUserAsync(x => x.Id == request.UserId, true,
                                                           request.IncludeInactiveUsers, cancellationToken);

            if (user == null)
            {
                return Result<UserResponse>.Failure(errorFactory.Create(new UserNotFound()));
            }

            UserResponse response = UserResponse.FromEntity(user);

            return Result<UserResponse>.Success(response);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Operation cancelled for user {userId}", request.UserId);

            return Result<UserResponse>.Failure(errorFactory.Create(new OperationCanceled()));
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);

            return Result<UserResponse>.Failure(errorFactory.Create(new InternalError()));
        }
    }
}