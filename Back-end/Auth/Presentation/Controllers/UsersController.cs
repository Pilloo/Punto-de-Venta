using System.ComponentModel.DataAnnotations;
using AuthModule.Core;
using AuthModule.Core.Features;
using AuthModule.Core.Features.UserFeatures;
using DTOs;
using DTOs.Users;
using ErrorHandling;
using ErrorHandling.Service;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthModule.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("/api/[controller]")]
[Tags("User Service API")]
public class UsersController(IMediator mediator, ErrorFactory errorFactory) : Controller
{
    [HttpPost]
    [Authorize]
    [EndpointDescription("Creates a user based on the provided credentials.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request,
                                              CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(errorFactory.Create(new ValidationFailed(ModelState)));
        }

        RegisterCommand command = RegisterCommand.FromDto(request);

        Result<Unit> result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.Status switch
            {
                (int)HttpCodes.Conflict => Conflict(result.Error),
                (int)HttpCodes.BadRequest => BadRequest(result.Error),
                _ => StatusCode((int)result.Error.Status!, result.Error)
            };
        }

        return Ok();
    }

    [HttpPatch]
    [Authorize]
    [EndpointDescription("Modifies the user information.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ModifyUser([FromBody] ModifyUserRequest request,
                                                CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(errorFactory.Create(new ValidationFailed(ModelState)));
        }

        ModifyUserCommand command = ModifyUserCommand.FromDto(request);

        Result<Unit> result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.Status switch
            {
                (int)HttpCodes.NotFound => NotFound(result.Error),
                _ => StatusCode((int)result.Error.Status!, result.Error)
            };
        }

        return Ok(result.Value);
    }

    [HttpGet]
    [Authorize]
    [Route("{id:guid}")]
    [EndpointDescription("Returns the user with the given id.")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserById([FromRoute] [Required] Guid? id, [FromQuery] [Required] bool? active,
                                                 CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(errorFactory.Create(new ValidationFailed(ModelState)));
        }

        GetUserByIdQuery query = GetUserByIdQuery.FromQueryParams(id!.Value, active!.Value);

        Result<UserResponse> result = await mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.Status switch
            {
                (int)HttpCodes.NotFound => NotFound(result.Error),
                _ => StatusCode((int)result.Error.Status!, result.Error)
            };
        }

        return Ok(result.Value);
    }

    [HttpGet]
    [Authorize]
    [EndpointDescription("")]
    [ProducesResponseType(typeof(PagedResult<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUsers([FromQuery] GetAllUserRequest request,
                                              CancellationToken cancellationToken)
    {
        GetAllUsersQuery query = GetAllUsersQuery.FromQueryParams(request);

        Result<PagedResult<UserResponse>> result = await mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.Status switch
            {
                (int)HttpCodes.NotFound => NotFound(result.Error),
                _ => StatusCode((int)result.Error.Status!, result.Error)
            };
        }

        return Ok(result.Value);
    }

    [HttpPatch]
    [Authorize]
    [Route("{id:guid}/status")]
    [EndpointDescription("Sets the account status for a given user.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetUserStatus(Guid id, [FromBody] SetUserStatusRequest request,
                                                   CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(errorFactory.Create(new ValidationFailed(ModelState)));
        }

        SetUserStatusCommand command = SetUserStatusCommand.FromDto(request);

        Result<Unit> result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.Status switch
            {
                (int)HttpCodes.NotFound => NotFound(result.Error),
                _ => StatusCode((int)result.Error.Status!, result.Error)
            };
        }

        return Ok();
    }
}