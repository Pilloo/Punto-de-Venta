using AuthModule.Core;
using AuthModule.Core.Features;
using AuthModule.Core.Interfaces;
using DTOs.Auth;
using ErrorHandling;
using ErrorHandling.Service;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace AuthModule.Presentation.Controllers
{
    [Route("/api/[controller]")]
    [Tags("Auth Service API")]
    [ApiController]
    public class AuthController(IMediator mediator, ErrorFactory errorFactory) : Controller
    {
        [HttpPost]
        [AllowAnonymous]
        [Route("login")]
        [EndpointDescription("Logs into an account using the provided credentials.")]
        [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginCommand loginCommand, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(errorFactory.Create(new ValidationFailed(ModelState)));
            }

            Result<TokenDto> result = await mediator.Send(loginCommand, cancellationToken);

            if (!result.IsSuccess)
            {
                return result.Error!.Status switch
                {
                    (int)ErrorCodes.Unauthorized => Unauthorized(result.Error),
                    _ => StatusCode((int)result.Error.Status!, result.Error)
                };
            }

            return Ok(result.Value);
        }

        [HttpPost]
        [Authorize]
        [Route("create-user")]
        [EndpointDescription("Creates a user based on the provided credentials.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterCommand registerCommand, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(errorFactory.Create(new ValidationFailed(ModelState)));
            }

            Result<Unit> result = await mediator.Send(registerCommand, cancellationToken);

            if (!result.IsSuccess)
            {
                return result.Error!.Status switch
                {
                    (int)ErrorCodes.Conflict => Conflict(result.Error),
                    (int)ErrorCodes.BadRequest => BadRequest(result.Error),
                    _ => StatusCode((int)result.Error.Status!, result.Error)
                };
            }

            return Ok();
        }

        [HttpPatch]
        [Authorize]
        [Route("modify-user")]
        [EndpointDescription("Modifies the user information.")]
        [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ModifyUser([FromBody] ModifyUserCommand modifyUserCommand, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(errorFactory.Create(new ValidationFailed(ModelState)));
            }

            Result<TokenDto> result = await mediator.Send(modifyUserCommand, cancellationToken);

            if (!result.IsSuccess)
            {
                return result.Error!.Status switch
                {
                    (int)ErrorCodes.NotFound => NotFound(result.Error),
                    _ => StatusCode((int)result.Error.Status!, result.Error)
                };
            }

            return Ok(result.Value);
        }

        [HttpPost]
        [Authorize]
        [Route("refresh-token")]
        [EndpointDescription("Refreshes the access token and issues a new refresh token.")]
        [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshAccessTokenCommand refreshCommand, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(errorFactory.Create(new ValidationFailed(ModelState)));
            }

            Result<TokenDto> result = await mediator.Send(refreshCommand, cancellationToken);

            if (!result.IsSuccess)
            {
                return result.Error!.Status switch
                {
                    (int)ErrorCodes.ValidationError => UnprocessableEntity(result.Error),
                    _ => StatusCode((int)result.Error.Status!, result.Error)
                };
            }

            return Ok(result.Value);
        }

        [HttpPatch]
        [Authorize]
        [Route("set-user-status")]
        [EndpointDescription("Sets the account status for a certain user.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SetUserStatus([FromBody] SetUserStatusCommand setUserStatusCommand, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(errorFactory.Create(new ValidationFailed(ModelState)));
            }

            Result<Unit> result = await mediator.Send(setUserStatusCommand, cancellationToken);

            if (!result.IsSuccess)
            {
                return result.Error!.Status switch
                {
                    (int)ErrorCodes.NotFound => NotFound(result.Error),
                    _ => StatusCode((int)result.Error.Status!, result.Error)
                };
            }

            return Ok();
        }

        [HttpGet]
        [Authorize]
        [Route("get-active-users")]
        [EndpointDescription("Returns the active user list.")]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetActiveUsers(CancellationToken cancellationToken)
        {
            Result<IEnumerable<UserDto>> result = await mediator.Send(new GetActiveUsersQuery(), cancellationToken);

            if (!result.IsSuccess)
            {
                return result.Error!.Status switch
                {
                    (int)ErrorCodes.NotFound => NotFound(result.Error),
                    _ => StatusCode((int)result.Error.Status!, result.Error)
                };
            }

            return Ok(result.Value);
        }

        [HttpGet]
        [Authorize]
        [Route("get-inactive-users")]
        [EndpointDescription("Returns the active user list.")]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInactiveUsers(CancellationToken cancellationToken)
        {
            Result<IEnumerable<UserDto>> result = await mediator.Send(new GetInactiveUsersQuery(), cancellationToken);

            if (!result.IsSuccess)
            {
                return result.Error!.Status switch
                {
                    (int)ErrorCodes.NotFound => NotFound(result.Error),
                    _ => StatusCode((int)result.Error.Status!, result.Error)
                };
            }

            return Ok(result.Value);
        }
    }

    [Route(".well-known")]
    [Tags(".well-known API")]
    [ApiController]
    public class WellKnownController(IConfiguration configuration) : Controller
    {
        [HttpGet("jwks.json")]
        public IActionResult GetJwks(ICryptoService cryptoService)
        {
            IConfigurationSection jwtOptions = configuration.GetSection("Jwt");

            ECDsa key = cryptoService.LoadEcdsaKey(jwtOptions.GetValue<string>("PublicKeyPath")!);

            ECDsaSecurityKey ecdsaSecurityKey = new ECDsaSecurityKey(key);

            JsonWebKey jsonWebKey = JsonWebKeyConverter.ConvertFromECDsaSecurityKey(ecdsaSecurityKey);

            jsonWebKey.Kid = Convert.ToHexString(ecdsaSecurityKey.ComputeJwkThumbprint());
            
            jsonWebKey.Use = "sig";

            var jwks = new { keys = new[] { jsonWebKey }};

            return Ok(jwks);
        }
    }
}
