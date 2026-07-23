using AuthModule.Core;
using AuthModule.Core.Interfaces;
using DTOs.Auth;
using ErrorHandling;
using ErrorHandling.Service;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using AuthModule.Core.Features.AuthFeatures;

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
        [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(errorFactory.Create(new ValidationFailed(ModelState)));
            }

            LoginCommand command = LoginCommand.FromDto(request);

            Result<TokenResponse> result = await mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
            {
                return result.Error!.Status switch
                {
                    (int)HttpCodes.Unauthorized => Unauthorized(result.Error),
                    _ => StatusCode((int)result.Error.Status!, result.Error)
                };
            }

            return Ok(result.Value);
        }

        [HttpPost]
        [Authorize]
        [Route("refresh-token")]
        [EndpointDescription("Refreshes the access token and issues a new refresh token.")]
        [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshAccessTokenRequest request,
                                                      CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(errorFactory.Create(new ValidationFailed(ModelState)));
            }

            RefreshAccessTokenCommand command = RefreshAccessTokenCommand.FromDto(request);

            Result<TokenResponse> result = await mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
            {
                return result.Error!.Status switch
                {
                    (int)HttpCodes.ValidationError => UnprocessableEntity(result.Error),
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

            var jwks = new { keys = new[] { jsonWebKey } };

            return Ok(jwks);
        }
    }
}