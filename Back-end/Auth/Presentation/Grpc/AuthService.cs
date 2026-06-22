using AuthModule.Core.Features;
using AuthService.Protos;
using ErrorHandling;
using Grpc.Core;
using MediatR;
using Models;
using static AuthService.Protos.AuthService;

namespace AuthModule.Presentation.Grpc;

public class AuthGrpcService(IMediator mediator) : AuthServiceBase
{
    public override async Task<UserInfo> GetUserInfo(UserRequest request, ServerCallContext context)
    {
        GetUserQuery query = new()
        {
            UserId = Guid.Parse(request.UserId),
        };

        Result<User?> result = await mediator.Send(query);
        
        User? user = result.Value;

        if (user == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"User {request.UserId} not found."));
        }

        UserInfo userInfo = new()
        {
            GivenName = user.GivenName,
            LastName = user.LastName,
            IsActive = user.IsActive,
        };
        
        return userInfo;
    }
}