using DTOs;
using DTOs.Inventory.Product;
using ErrorHandling;
using ErrorHandling.Service;
using Inventory.Core;
using Inventory.Core.Features.ProductFeatures;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Presentation.Controllers;

[Route("/api/inventory/[controller]")]
[ApiController]
[Tags("Product API")]
[Authorize]
public class ProductsController(IMediator mediator, ErrorFactory errorFactory) : Controller
{
    [HttpPost]
    [RequestTimeout(2000)]
    [EndpointDescription("Creates a new product.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status499ClientClosedRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status499ClientClosedRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request,
                                            CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(errorFactory.Create(new ValidationFailed(ModelState)));
        }

        CreateProductCommand command = CreateProductCommand.FromDto(request);

        Result<Unit> result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error?.Status switch
            {
                (int)HttpCodes.Conflict => Conflict(result.Error!),
                (int)HttpCodes.OperationCancelled => StatusCode((int)result.Error.Status, result.Error),
                _ => StatusCode((int)result.Error!.Status!, result.Error)
            };
        }

        return Ok();
    }

    [HttpGet]
    [RequestTimeout(10000)]
    [EndpointDescription("Returns all products.")]
    [ProducesResponseType(typeof(PagedResult<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status499ClientClosedRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(GetAllProductsRequest request, CancellationToken cancellationToken)
    {
        GetAllProductsQuery query = GetAllProductsQuery.FromQueryParams(request);

        Result<PagedResult<ProductResponse>> result = await mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error?.Status switch
            {
                _ => StatusCode((int)result.Error!.Status!, result.Error),
            };
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [RequestTimeout(15000)]
    [Route("search")]
    [EndpointDescription("Returns all products that match the search criteria.")]
    [ProducesResponseType(typeof(PagedResult<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status499ClientClosedRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByFilter(GetProductsQueryFilter filter, CancellationToken cancellationToken)
    {
        GetProductsByFilterQuery query = GetProductsByFilterQuery.FromDto(filter);

        Result<PagedResult<ProductResponse>> result = await mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error?.Status switch
            {
                _ => StatusCode((int)result.Error!.Status!, result.Error)
            };
        }

        return Ok(result.Value);
    }

    [HttpGet]
    [RequestTimeout(10000)]
    [Route("{id:guid}")]
    [EndpointDescription("Returns the product with the given id.")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status499ClientClosedRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        GetProductByIdQuery query = GetProductByIdQuery.FromQueryParams(id);

        Result<ProductResponse> result = await mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error?.Status switch
            {
                (int)HttpCodes.NotFound => NotFound(result.Error),
                _ => StatusCode((int)result.Error!.Status!, result.Error)
            };
        }

        return Ok(result.Value);
    }

    [HttpPatch]
    [RequestTimeout(5000)]
    [Route("{id:guid}")]
    [EndpointDescription("Modifies a product.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status499ClientClosedRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Modify(ModifyProductRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(errorFactory.Create(new ValidationFailed(ModelState)));
        }

        ModifyProductCommand command = ModifyProductCommand.FromDto(request);

        Result<Unit> result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.Status! switch
            {
                (int)HttpCodes.NotFound => NotFound(result.Error),
                _ => StatusCode((int)result.Error.Status!, result.Error)
            };
        }

        return Ok();
    }

    [HttpPatch]
    [Route("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status499ClientClosedRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ModifyStatus([FromRoute] Guid id, [FromBody] SetProductStatusRequest request,
                                                  CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(errorFactory.Create(new ValidationFailed(ModelState)));
        }

        SetProductStatusCommand command = SetProductStatusCommand.FromDto(id, request);

        Result<Unit> result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.Status! switch
            {
                (int)HttpCodes.NotFound => NotFound(result.Error),
                _ => StatusCode((int)result.Error.Status!, result.Error)
            };
        }

        return Ok();
    }
}