using AuthModule.Core.Interfaces;
using DTOs;
using DTOs.Users;
using ErrorHandling;
using ErrorHandling.Service;
using MediatR;
using Microsoft.Extensions.Logging;
using Models.Auth;

namespace AuthModule.Core.Features.UserFeatures;

/// <summary>
/// Represents a query to retrieve a paginated list of user data with optional filtering for active or inactive users.
/// </summary>
/// <remarks>
/// This query is designed to handle scenarios where user information needs to be fetched
/// with pagination and filtering by the user's active status. The query returns a result containing
/// a <see cref="Result{T}" /> object that wraps a <see cref="PagedResult{T}" /> of <see cref="UserResponse" />.
/// </remarks>
/// <example>
/// This query can be constructed using query parameters from a <see cref="GetAllUserRequest" /> object
/// by calling the static factory method <see cref="GetAllUsersQuery.FromQueryParams" />.
/// </example>
/// <seealso cref="GetAllUserRequest" />
/// <seealso cref="UserResponse" />
/// <seealso cref="Result{T}" />
/// <seealso cref="PagedResult{T}" />
public record GetAllUsersQuery : IRequest<Result<PagedResult<UserResponse>>>
{
    public bool? Active { get; private init; }
    public bool? IncludeInactive { get; private init; }
    public int PageNumber { get; private init; } = 1;
    public int PageSize { get; private init; } = 10;

    public static GetAllUsersQuery FromQueryParams(GetAllUserRequest request) => new()
    {
        Active = request.Active,
        IncludeInactive = request.IncludeInactive,
        PageNumber = request.PageNumber,
        PageSize = request.PageSize
    };
}

public class GetAllUsersQueryHandler(
    IUserRepository userRepository,
    ErrorFactory errorFactory,
    ILogger<GetAllUsersQueryHandler> logger)
    : IRequestHandler<GetAllUsersQuery, Result<PagedResult<UserResponse>>>
{
    public async Task<Result<PagedResult<UserResponse>>> Handle(GetAllUsersQuery request,
                                                                CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            (IReadOnlyCollection<User> users, int userCount) =
                await userRepository.GetAllUsersAsync(request.Active, request.IncludeInactive, request.PageNumber,
                                                      request.PageSize, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            IReadOnlyCollection<UserResponse> dtoList = users.Select(UserResponse.FromEntity).ToList();

            PagedResult<UserResponse> result = new PagedResult<UserResponse>()
            {
                Items = dtoList,
                TotalCount = userCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return Result<PagedResult<UserResponse>>.Success(result);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Data fetch operation canceled.");

            return Result<PagedResult<UserResponse>>.Failure(errorFactory.Create(new OperationCanceled()));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);

            return Result<PagedResult<UserResponse>>.Failure(errorFactory.Create(new InternalError()));
        }
    }
}