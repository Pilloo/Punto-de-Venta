using AuthModule.Core.Interfaces;
using DTOs;
using ErrorHandling;
using ErrorHandling.Service;
using MediatR;
using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuthModule.Core.Features;

public class GetUsersQuery : IRequest<Result<IEnumerable<UserDto>>>
{

}

public class GetActiveUsersQueryHandler(IUserRepository userRepository, ErrorFactory errorFactory, Logger<GetActiveUsersQueryHandler> logger) : IRequestHandler<GetUsersQuery, Result<IEnumerable<UserDto>>>
{
    public async Task<Result<IEnumerable<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<User> userList = await userRepository.GetUsersAsync(cancellationToken);

            IEnumerable<UserDto> dtoList = from user in userList
                                           orderby user.LastName descending
                                           select new UserDto
                                           {
                                               UserId = user.Id,
                                               UserName = user.UserName!,
                                               GivenName = user.GivenName,
                                               LastName = user.LastName
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