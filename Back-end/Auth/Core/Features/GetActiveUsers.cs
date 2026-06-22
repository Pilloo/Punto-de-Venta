using AuthModule.Core.Interfaces;
using DTOs.Auth;
using ErrorHandling;
using ErrorHandling.Service;
using MediatR;
using Microsoft.Extensions.Logging;
using Models;

namespace AuthModule.Core.Features;

/// <summary>
/// Request to retrieve active users as a Result<IEnumerable<UserDto>>.
/// </summary>
/// <remarks>Intended for use with MediatR; a handler should return a Result<IEnumerable<UserDto>> containing the
/// active users.</remarks>
public class GetActiveUsersQuery : IRequest<Result<IEnumerable<UserDto>>>
{

}

public class GetActiveUsersQueryHandler(IUserRepository userRepository, ErrorFactory errorFactory, ILogger<GetActiveUsersQueryHandler> logger) : IRequestHandler<GetActiveUsersQuery, Result<IEnumerable<UserDto>>>
{
    public async Task<Result<IEnumerable<UserDto>>> Handle(GetActiveUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<User?> userList = await userRepository.GetUsersAsync(x => x.IsActive, true, cancellationToken);

            if (!userList.Any())
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