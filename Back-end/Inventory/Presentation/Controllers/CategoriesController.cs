using System.ComponentModel.DataAnnotations;
using DTOs;
using DTOs.Inventory.category;
using DTOs.Inventory.Category;
using ErrorHandling;
using ErrorHandling.Service;
using Inventory.Core;
using Inventory.Core.Features.CategoryFeatures;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace Inventory.Presentation.Controllers;

[Route("/api/inventory/[controller]")]
[ApiController]
[Tags("Category API")]
[Authorize]
public class CategoriesController(IMediator mediator, ErrorFactory errorFactory) : Controller
{
    [HttpPost]
    [EndpointDescription("Creates a new category.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] [Required] CreateCategoryRequest request,
                                            CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(errorFactory.Create(new ValidationFailed(ModelState)));
        }

        CreateCategoryCommand command = CreateCategoryCommand.FromDto(request);

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
    [EndpointDescription("Returns the category with the given id.")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById([Required] [FromQuery] Guid id, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(errorFactory.Create(new ValidationFailed(ModelState)));
        }

        GetCategoryByIdQuery query = GetCategoryByIdQuery.FromQueryParams(id);

        Result<CategoryResponse> result = await mediator.Send(query, cancellationToken);

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
    [EndpointDescription("Returns all categories.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PagedResult<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllCategoriesRequest request,
                                            CancellationToken cancellationToken)
    {
        GetAllCategoriesQuery query = GetAllCategoriesQuery.FromQueryParams(request);

        Result<PagedResult<CategoryResponse>> result = await mediator.Send(query, cancellationToken);

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
    [EndpointDescription("Modifies a category.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Modify([Required] Guid id, [Required] ModifyCategoryRequest request,
                                            CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(errorFactory.Create(new ValidationFailed(ModelState)));
        }

        ModifyCategoryCommand command = ModifyCategoryCommand.FromDto(id, request);

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
    [EndpointDescription("Sets the status of a category.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetStatus([Required] Guid id, [Required] SetCategoryStatusRequest request,
                                               CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(errorFactory.Create(new ValidationFailed(ModelState)));
        }

        SetCategoryStatusCommand command = SetCategoryStatusCommand.FromDto(id, request);

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