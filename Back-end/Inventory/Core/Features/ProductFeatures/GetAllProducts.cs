using DTOs;
using DTOs.Inventory.Product;
using ErrorHandling;
using ErrorHandling.Service;
using Inventory.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Models;

namespace Inventory.Core.Features.ProductFeatures;

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