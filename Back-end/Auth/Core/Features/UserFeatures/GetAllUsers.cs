using AuthModule.Core.Interfaces;
using DTOs;
using DTOs.Users;
using ErrorHandling;
using ErrorHandling.Service;
using MediatR;
using Microsoft.Extensions.Logging;
using Models;

namespace AuthModule.Core.Features.UserFeatures;

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