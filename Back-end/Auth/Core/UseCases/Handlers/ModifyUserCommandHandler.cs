using Core.UseCases.Commands;
using DTOs;
using ErrorHandling;
using ErrorHandling.Service;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.UseCases.Handlers
{
    public class ModifyUserCommandHandler(
        UserManager<User> userManager,
        ErrorFactory errorFactory,
        ILogger<ModifyUserCommandHandler> logger
    ) : IRequestHandler<ModifyUserCommand, Result<LoginResponse>>
    {
        public async Task<Result<LoginResponse>> Handle(ModifyUserCommand command, CancellationToken ct)
        {

        }
    }
}
