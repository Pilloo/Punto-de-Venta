using Core;
using Core.UseCases.Commands;
using DTOs;
using ErrorHandling;
using ErrorHandling.Service;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController(IMediator mediator, IConfiguration configuration, ErrorFactory errorFactory) : Controller
    {
        [HttpPost]
        [AllowAnonymous]
        [Route("login")]
        [EndpointDescription("Logs into an account using the provided credentials.")]
        [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginCommand loginCommand, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(errorFactory.CreateProblemDetails(new ValidationFailed(ModelState)));
            }

            var response = await mediator.Send(loginCommand, cancellationToken);

            if (!response.IsSuccess)
            {
                return response.Error!.Status switch
                {
                    (int)ErrorCodes.Unauthorized => Unauthorized(response.Error),
                    _ => StatusCode((int)response.Error.Status!, response.Error)
                };
            }

            return Ok(response.Value);
        }
    }
}
