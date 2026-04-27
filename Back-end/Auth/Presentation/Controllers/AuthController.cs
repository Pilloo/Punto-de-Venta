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
    public class AuthController(IMediator mediator, ErrorFactory errorFactory) : Controller
    {
        [HttpPost]
        [AllowAnonymous]
        [Route("login")]
        [EndpointDescription("Logs into an account using the provided credentials.")]
        [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK, Type = typeof(LoginResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginCommand loginCommand, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(errorFactory.Create(new ValidationFailed(ModelState)));
            }

            Result<LoginResponse> response = await mediator.Send(loginCommand, cancellationToken);

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

        [HttpPost]
        [AllowAnonymous]
        [Route("create-user")]
        [EndpointDescription("Creates a user based on the provided credentials.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterCommand registerCommand, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(errorFactory.Create(new ValidationFailed(ModelState)));
            }

            Result<Unit> response = await mediator.Send(registerCommand, cancellationToken);

            if (!response.IsSuccess)
            {
                return response.Error!.Status switch
                {
                    (int)ErrorCodes.Conflict => Conflict(response.Error),
                    (int)ErrorCodes.BadRequest => BadRequest(response.Error),
                    _ => StatusCode((int)response.Error.Status!, response.Error)
                };
            }

            return Ok();
        }

        [HttpPost]
        [Authorize]
        [Route("refresh-token")]
        [EndpointDescription("Refreshes the access token and issues a new refresh token.")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponse))]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshAccessTokenCommand refreshCommand, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(errorFactory.Create(new ValidationFailed(ModelState)));
            }

            Result<LoginResponse> response = await mediator.Send(refreshCommand, cancellationToken);

            if (!response.IsSuccess)
            {
                return response.Error!.Status switch
                {
                    (int)ErrorCodes.ValidationError => UnprocessableEntity(response.Error),
                    _ => StatusCode((int)response.Error.Status!, response.Error)
                };
            }

            return Ok(response.Value);
        }
    }
}
