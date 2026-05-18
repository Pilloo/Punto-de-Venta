using AuthModule.Core.Features;
using DTOs;
using ErrorHandling;
using ErrorHandling.Service;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace AuthModule.Core.Pipelines
{
    public class ModifyUserValidationPipeline(
        IHttpContextAccessor httpContextAccessor, 
        ErrorFactory errorFactory, 
        ILogger<ModifyUserValidationPipeline> logger
    ) : IPipelineBehavior<ModifyUserCommand, Result<TokenDto>>
    {
        public async Task<Result<TokenDto>> Handle(ModifyUserCommand command, RequestHandlerDelegate<Result<TokenDto>> next, CancellationToken ct)
        {
            try
            {
                HttpContext? context = httpContextAccessor.HttpContext;

                ct.ThrowIfCancellationRequested();

                // Command is invoked from inside the module.
                if (context == null)
                {
                    return await next();
                }

                ClaimsPrincipal user = context.User;

                if (user.Identity?.IsAuthenticated == false)
                {
                    return Result<TokenDto>.Failure(errorFactory.Create(new UnauthorizedOperation()));
                }

                string? subjectId = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(subjectId) || Guid.Parse(subjectId) != command.UserId)
                {
                    return Result<TokenDto>.Failure(errorFactory.Create(new UnauthorizedOperation()));
                }

                return await next();
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Operation canceled for User Id {userId}", command.UserId);

                return Result<TokenDto>.Failure(errorFactory.Create(new OperationCanceled()));
            } catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);

                return Result<TokenDto>.Failure(errorFactory.Create(new InternalError()));
            }
        }
    }
}
