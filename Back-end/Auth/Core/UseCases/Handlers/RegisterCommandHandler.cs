using Core.UseCases.Commands;
using ErrorHandling;
using ErrorHandling.Service;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.UseCases.Handlers
{
    public class RegisterCommandHandler(UserManager<User> userManager, ErrorFactory errorFactory) : IRequestHandler<RegisterCommand, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(RegisterCommand command, CancellationToken ct)
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
    }
}
