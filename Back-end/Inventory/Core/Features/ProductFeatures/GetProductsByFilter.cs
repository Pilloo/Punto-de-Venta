using System.Linq.Expressions;
using DTOs;
using DTOs.Inventory.Product;
using ErrorHandling;
using ErrorHandling.Service;
using Inventory.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Models;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Core.Features.ProductFeatures;

public class GetProductsByFilterQuery : IRequest<Result<PagedResult<ProductResponse>>>
{
    public GetProductsQueryFilter Filter { get; init; } = new();

    public static GetProductsByFilterQuery FromDto(GetProductsQueryFilter filter) => new()
    {
        Filter = filter
    };
}

public class GetProductsByFilterQueryHandler(
    IProductRepository repository,
    ErrorFactory errorFactory,
    ILogger<GetProductsByFilterQueryHandler> logger)
    : IRequestHandler<GetProductsByFilterQuery, Result<PagedResult<ProductResponse>>>
{
    public async Task<Result<PagedResult<ProductResponse>>> Handle(GetProductsByFilterQuery query,
                                                                   CancellationToken cancellationToken)
    {
        try
        {
            // Default expression: 1 = 1
            ExpressionStarter<Product> predicate = PredicateBuilder.New<Product>(true);

            if (query.Filter.SearchTerm != null)
            {
                predicate.And(product => product.BarcodeContent.Contains(query.Filter.SearchTerm) ||
                                         EF.Functions.FreeText(product.ItemSummary, query.Filter.SearchTerm));
            }

            if (query.Filter.BrandId != null)
            {
                predicate.And(product => product.BrandId == query.Filter.BrandId);
            }

            if (query.Filter.ColourId != null)
            {
                predicate.And(product => product.ColourId == query.Filter.ColourId);
            }

            if (query.Filter.CategoryId != null)
            {
                predicate.And(product => product.CategoryId == query.Filter.CategoryId);
            }

            if (query.Filter.PriceRange != null)
            {
                predicate.And(product => product.Price >= query.Filter.PriceRange.Min &&
                                         product.Price <= query.Filter.PriceRange.Max);
            }

            if (query.Filter.StockCountRange != null)
            {
                predicate.And(product => product.StockCount >= query.Filter.StockCountRange.Min &&
                                         product.StockCount <= query.Filter.StockCountRange.Max);
            }

            if (query.Filter.IsActive != null)
            {
                predicate.And(product => product.IsActive == query.Filter.IsActive);
            }

            (IReadOnlyCollection<Product> items, int totalCount) =
                await repository.GetProductsAsync(predicate, true, true, query.Filter.PageNumber, query.Filter.PageSize,
                                                  cancellationToken);

            IReadOnlyList<ProductResponse> dtoList = items.Select(ProductResponse.FromEntity).ToList();

            PagedResult<ProductResponse> currentPage = new()
            {
                Items = dtoList,
                TotalCount = totalCount,
                PageSize = query.Filter.PageSize,
                PageNumber = query.Filter.PageNumber
            };

            return Result<PagedResult<ProductResponse>>.Success(currentPage);
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