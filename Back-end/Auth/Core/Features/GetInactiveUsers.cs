using AuthModule.Core.Interfaces;
using DTOs.Auth;
using ErrorHandling;
using ErrorHandling.Service;
using MediatR;
using Microsoft.Extensions.Logging;
using Models;

namespace AuthModule.Core.Features;

/// <summary>
/// Represents a query to retrieve inactive users.
/// </summary>
/// <remarks>Handled by a MediatR handler and returns a Result<IEnumerable<UserDto>>.</remarks>
public class GetInactiveUsersQuery : IRequest<Result<IEnumerable<UserDto>>>
{

}

public class GetInactiveUsersQueryHandler(IUserRepository userRepository, ErrorFactory errorFactory, ILogger<GetInactiveUsersQueryHandler> logger) : IRequestHandler<GetInactiveUsersQuery, Result<IEnumerable<UserDto>>>
{
    public async Task<Result<IEnumerable<UserDto>>> Handle(GetInactiveUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<User?> userList = await userRepository.GetUsersAsync(x => !x.IsActive, true, cancellationToken);

            if (userList.Count() == 0)
            {
                return Result<IEnumerable<UserDto>>.Failure(errorFactory.Create(new ActiveUsersNotFound()));
            }

            IEnumerable<UserDto> dtoList = from user in userList
                                           orderby user.LastName descending
                                           select new UserDto
                                           {
                                               UserId = user.Id,
                                               UserName = user.UserName!,
                                               GivenName = user.GivenName,
                                               LastName = user.LastName,
                                               IsActive = user.IsActive,
                                           };

            return Result<IEnumerable<UserDto>>.Success(dtoList);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Data fetch operation canceled.");

            return Result<IEnumerable<UserDto>>.Failure(errorFactory.Create(new OperationCanceled()));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);

            return Result<IEnumerable<UserDto>>.Failure(errorFactory.Create(new InternalError()));
        }
    }
}