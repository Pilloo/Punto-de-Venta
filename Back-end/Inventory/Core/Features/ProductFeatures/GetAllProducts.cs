using DTOs;
using DTOs.Inventory.Product;
using ErrorHandling;
using ErrorHandling.Service;
using Inventory.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Models.Inventory;

namespace Inventory.Core.Features.ProductFeatures;

/// <summary>
/// Represents a query to retrieve all products with optional filtering and pagination parameters.
/// </summary>
/// <remarks>
/// This query is processed through the MediatR pipeline and yields a <see cref="Result{T}"/>
/// object containing a paginated list of product data in the form of <see cref="PagedResult{T}"/>.
/// </remarks>
/// <example>
/// The query supports optional filtering parameters such as retrieving active products and
/// the inclusion of inactive ones. Results are controlled by the pagination parameters
/// <see cref="PageNumber"/> and <see cref="PageSize"/>.
/// </example>
public class GetAllProductsQuery : IRequest<Result<PagedResult<ProductResponse>>>
{
    public bool? Active { get; private init; }
    public bool? IncludeInactive { get; private init; }
    public int PageNumber { get; private init; } = 1;
    public int PageSize { get; private init; } = 10;

    public static GetAllProductsQuery FromQueryParams(GetAllProductsRequest request) => new()
    {
        Active = request.Active,
        IncludeInactive = request.IncludeInactive,
        PageNumber = request.PageNumber,
        PageSize = request.PageSize
    };
}

public class GetAllProductsQueryHandler(
    IProductRepository repository,
    ErrorFactory errorFactory,
    ILogger<GetAllProductsQueryHandler> logger)
    : IRequestHandler<GetAllProductsQuery, Result<PagedResult<ProductResponse>>>
{
    public async Task<Result<PagedResult<ProductResponse>>> Handle(GetAllProductsQuery request,
                                                                   CancellationToken cancellationToken)
    {
        try
        {
            (IReadOnlyCollection<Product> items, int totalCount) =
                await repository.GetAllProductsAsync(request.Active, request.IncludeInactive, request.PageNumber,
                                                     request.PageSize, cancellationToken);

            IReadOnlyList<ProductResponse> dtoList = items.Select(ProductResponse.FromEntity).ToList();

            PagedResult<ProductResponse> result = new PagedResult<ProductResponse>()
            {
                Items = dtoList,
                TotalCount = totalCount,
                PageSize = request.PageSize,
                PageNumber = request.PageNumber
            };

            return Result<PagedResult<ProductResponse>>.Success(result);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Data fetch operation canceled.");

            return Result<PagedResult<ProductResponse>>.Failure(errorFactory.Create(new OperationCanceled()));
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);

            return Result<PagedResult<ProductResponse>>.Failure(errorFactory.Create(new InternalError()));
        }
    }
}