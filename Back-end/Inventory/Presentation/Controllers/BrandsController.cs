using System.ComponentModel.DataAnnotations;
using DTOs;
using DTOs.Inventory.Brand;
using ErrorHandling;
using ErrorHandling.Service;
using Inventory.Core;
using Inventory.Core.Features.BrandFeatures;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace Inventory.Presentation.Controllers;

[Route("/api/inventory/[controller]")]
[ApiController]
[Tags("Brand API")]
[Authorize]
public class BrandsController(IMediator mediator, ErrorFactory errorFactory) : Controller
{
    [HttpPost]
    [EndpointDescription("Creates a new brand.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] [Required] CreateBrandRequest request,
                                            CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(errorFactory.Create(new ValidationFailed(ModelState)));
        }

        CreateBrandCommand command = CreateBrandCommand.FromDto(request);

        Result<Unit> result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.Status switch
            {
                (int)HttpCodes.Conflict => Conflict(result.Error!),
                (int)HttpCodes.Ok => Ok(result.Error!),
                _ => StatusCode((int)result.Error.Status!, result.Error)
            };
        }

        return Ok();
    }

    [HttpGet("{id:guid}")]
    [EndpointDescription("Returns the brand with the given id.")]
    [ProducesResponseType(typeof(Brand), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(errorFactory.Create(new ValidationFailed(ModelState)));
        }

        GetBrandByIdQuery query = new()
        {
            Id = id
        };

        Result<Brand?> result = await mediator.Send(query, cancellationToken);

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
    [EndpointDescription("Returns all brands.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PagedResult<BrandResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllBrandsRequest request,
                                            CancellationToken cancellationToken)
    {
        GetAllBrandsQuery query = GetAllBrandsQuery.FromQueryParams(request);

        Result<PagedResult<BrandResponse>> result = await mediator.Send(query, cancellationToken);

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

    [HttpPatch("{id:guid}")]
    [EndpointDescription("Modifies a brand.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Modify([Required] Guid id, [Required] ModifyBrandRequest request,
                                            CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(errorFactory.Create(new ValidationFailed(ModelState)));
        }

        ModifyBrandCommand command = ModifyBrandCommand.FromDto(id, request);

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
    [EndpointDescription("Sets the status of a brand.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetStatus(Guid id, SetBrandStatusRequest request,
                                               CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(errorFactory.Create(new ValidationFailed(ModelState)));
        }

        SetBrandStatusCommand command = SetBrandStatusCommand.FromDto(id, request);

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