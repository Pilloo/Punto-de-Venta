using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using DTOs;
using DTOs.Inventory;
using DTOs.Inventory.Brand;
using DTOs.Inventory.Colour;
using ErrorHandling;
using ErrorHandling.Service;
using Grpc.Core;
using Inventory.Core;
using Inventory.Core.Features.BrandFeatures;
using Inventory.Core.Features.ColourFeatures;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace Inventory.Presentation.Controllers;

[Route("/api/inventory/[controller]")]
[ApiController]
[Tags("Colour API")]
[Authorize]
public class ColoursController(IMediator mediator, ErrorFactory errorFactory) : Controller
{
    [HttpPost]
    [EndpointDescription("Creates a new colour.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] [Required] CreateColourRequest request,
                                            CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(errorFactory.Create(new ValidationFailed(ModelState)));
        }

        CreateColourCommand command = CreateColourCommand.FromDto(request);

        Result<Unit> result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.Status switch
            {
                (int)HttpCodes.Conflict => Conflict(result.Error!),
                _ => StatusCode((int)result.Error.Status!, result.Error)
            };
        }

        return Ok();
    }

    [HttpGet("{id:guid}")]
    [EndpointDescription("Returns the colour with the given id.")]
    [ProducesResponseType(typeof(ColourResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById([Required] Guid id, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(errorFactory.Create(new ValidationFailed(ModelState)));
        }

        GetColourByIdQuery query = new()
        {
            Id = id
        };

        Result<ColourResponse> result = await mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.Status switch
            {
                (int)HttpCodes.NotFound => NotFound(result.Error!),
                _ => StatusCode((int)result.Error.Status!, result.Error)
            };
        }

        return Ok(result.Value);
    }

    [HttpGet]
    [EndpointDescription("Returns all colours.")]
    [ProducesResponseType(typeof(PagedResult<ColourResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllColoursRequest request,
                                            CancellationToken cancellationToken)
    {
        GetAllColoursQuery query = GetAllColoursQuery.FromQueryParams(request);

        Result<PagedResult<ColourResponse>> result = await mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.Status switch
            {
                _ => StatusCode((int)result.Error.Status!, result.Error)
            };
        }

        return Ok(result.Value);
    }

    [HttpPatch("{id:guid}")]
    [EndpointDescription("Modifies a colour.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Modify([Required] Guid id, [Required] ModifyColourRequest request,
                                            CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(errorFactory.Create(new ValidationFailed(ModelState)));
        }

        ModifyColourCommand command = ModifyColourCommand.FromDto(id, request);

        Result<Unit> result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.Status switch
            {
                (int)HttpCodes.NotFound => NotFound(result.Error!),
                _ => StatusCode((int)result.Error.Status!, result.Error)
            };
        }

        return Ok();
    }

    [HttpPatch("{id:guid}/status")]
    [EndpointDescription("Sets the status of a colour.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetStatus([Required] Guid id, [Required] SetColourStatusRequest request,
                                               CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(errorFactory.Create(new ValidationFailed(ModelState)));
        }

        SetColourStatusCommand command = SetColourStatusCommand.FromDto(id, request);

        Result<Unit> result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.Status switch
            {
                (int)HttpCodes.NotFound => NotFound(result.Error!),
                _ => StatusCode((int)result.Error.Status!, result.Error)
            };
        }

        return Ok();
    }
}