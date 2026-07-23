using DTOs.Users;
using ErrorHandling;
using ErrorHandling.Service;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Models.Auth;

namespace AuthModule.Core.Features.UserFeatures;

/// <summary>
/// Represents a command to register a new user in the authentication system.
/// This command captures the necessary information required for user registration,
/// including username, given name, last name, and password.
/// </summary>
/// <remarks>
/// The <see cref="RegisterCommand"/> implements the <see cref="IRequest{TResponse}"/> interface from MediatR
/// and is designed to be processed by the corresponding request handler.
/// The command is created using the <see cref="FromDto"/> method, which maps a <see cref="RegisterRequest"/>
/// DTO to this command.
/// </remarks>
public record RegisterCommand : IRequest<Result<Unit>>
{
    public string UserName { get; private init; } = string.Empty;

    public string GivenName { get; private init; } = string.Empty;

    public string LastName { get; private init; } = string.Empty;

    public string Password { get; private init; } = string.Empty;

    public static RegisterCommand FromDto(RegisterRequest request) => new()
    {
        GivenName = request.GivenName,
        LastName = request.LastName,
        Password = request.Password,
        UserName = request.UserName
    }; 
}

public class RegisterCommandHandler(
    UserManager<User> userManager, 
    ErrorFactory errorFactory, 
    ILogger<RegisterCommandHandler> logger
) : IRequestHandler<RegisterCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(RegisterCommand command, CancellationToken ct)
    {
        try
        {
            if (await userManager.FindByNameAsync(command.UserName) != null)
            {
                return Result<Unit>.Failure(errorFactory.Create(new UserNameAlreadyTaken()));
            }

            User user = new User
            {
                UserName = command.UserName,
                GivenName = command.GivenName,
                LastName = command.LastName
            };

            ct.ThrowIfCancellationRequested();

            IdentityResult operationResult = await userManager.CreateAsync(user, command.Password);

            if (!operationResult.Succeeded)
            {
                return Result<Unit>.Failure(errorFactory.Create(new ValidationFailed(operationResult.Errors)));
            }

            return Result<Unit>.Success(Unit.Value);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Operation canceled for user register");

            return Result<Unit>.Failure(errorFactory.Create(new OperationCanceled()));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);

            return Result<Unit>.Failure(errorFactory.Create(new InternalError()));
        }
    }
}

