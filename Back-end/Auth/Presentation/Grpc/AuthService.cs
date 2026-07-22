using AuthModule.Core.Features.UserFeatures;
using AuthService.Protos;
using DTOs.Users;
using ErrorHandling;
using Grpc.Core;
using MediatR;
using static AuthService.Protos.AuthService;

namespace AuthModule.Presentation.Grpc;

public class AuthGrpcService(IMediator mediator, ILogger<AuthGrpcService> logger) : AuthServiceBase
{
    public override async Task<UserInfo> GetUserInfo(UserRequest request, ServerCallContext context)
    {
        try
        {
            GetUserByIdQuery byIdQuery = new()
            {
                UserId = Guid.Parse(request.UserId),
                IncludeInactiveUsers = request.IncludeInactiveUsers
            };

            Result<UserResponse> result = await mediator.Send(byIdQuery);

            if (!result.IsSuccess && result.Error!.Status == 404)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"User {request.UserId} not found."));
            }

            UserInfo userInfo = new()
            {
                GivenName = result.Value.GivenName,
                LastName = result.Value.LastName,
                IsActive = result.Value.IsActive,
            };

            return userInfo;
        }
        catch (RpcException e)
        {
            logger.LogDebug(e, e.Message);
            
            throw;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            
            throw new RpcException(new Status(StatusCode.Internal, "An unexpected error occurred."));
        }
    }
}