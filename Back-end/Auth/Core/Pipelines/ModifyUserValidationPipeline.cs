using DTOs.Auth;
using ErrorHandling;
using ErrorHandling.Service;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using AuthModule.Core.Features.UserFeatures;

namespace AuthModule.Core.Pipelines
{
    /// <summary>
    /// Validates that the current HTTP user is authenticated and that the user's subject identifier matches the
    /// ModifyUserCommand.UserId before allowing the pipeline to continue; returns appropriate error results for
    /// unauthorised access, cancellation, or internal failures.
    /// </summary>
    /// <remarks>Short-circuits when invoked inside the module (no HttpContext), when the user is
    /// unauthenticated, or when the authenticated user's subject identifier does not match the command UserId. Catches
    /// OperationCanceledException and logs cancellation; catches other exceptions and returns an internal
    /// error.</remarks>
    /// <param name="httpContextAccessor">Provides access to the current HttpContext to retrieve the authenticated user.</param>
    /// <param name="errorFactory">Creates standardised error instances used when validation fails or exceptions occur.</param>
    /// <param name="logger">Logs informational and error events during validation and exception handling.</param>
    public class ModifyUserValidationPipeline(
        IHttpContextAccessor httpContextAccessor,
        ErrorFactory errorFactory,
        ILogger<ModifyUserValidationPipeline> logger
    ) : IPipelineBehavior<ModifyUserCommand, Result<TokenResponse>>
    {
        public async Task<Result<TokenResponse>> Handle(ModifyUserCommand command,
                                                        RequestHandlerDelegate<Result<TokenResponse>> next,
                                                        CancellationToken ct)
        {
            try
            {
                HttpContext? context = httpContextAccessor.HttpContext;

                ct.ThrowIfCancellationRequested();

                // Command is invoked from inside the module.
                if (context == null)
                {
                    return await next(ct);
                }

                ClaimsPrincipal user = context.User;

                if (user.Identity?.IsAuthenticated == false)
                {
                    return Result<TokenResponse>.Failure(errorFactory.Create(new UnauthorizedOperation()));
                }

                string? subjectId = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(subjectId) || Guid.Parse(subjectId) != command.UserId)
                {
                    return Result<TokenResponse>.Failure(errorFactory.Create(new UnauthorizedOperation()));
                }

                return await next(ct);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Operation canceled for User Id {userId}", command.UserId);

                return Result<TokenResponse>.Failure(errorFactory.Create(new OperationCanceled()));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);

                return Result<TokenResponse>.Failure(errorFactory.Create(new InternalError()));
            }
        }
    }
}