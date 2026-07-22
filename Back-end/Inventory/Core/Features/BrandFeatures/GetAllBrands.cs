using DTOs;
using DTOs.Inventory.Brand;
using ErrorHandling;
using ErrorHandling.Service;
using Inventory.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Models;

namespace Inventory.Core.Features.BrandFeatures;

/// <summary>
/// Represents a query to retrieve a paginated list of all brands in the inventory system.
/// </summary>
/// <remarks>
/// This query supports optional filtering options such as retrieving active brands,
/// including inactive brands, and specifying pagination parameters like page number and page size.
/// </remarks>
public record GetAllBrandsQuery : IRequest<Result<PagedResult<BrandResponse>>>
{
    public bool? Active { get; private init; }
    public bool? IncludeInactive { get; private init; }
    public int PageNumber { get; private init; } = 1;
    public int PageSize { get; private init; } = 10;

    public static GetAllBrandsQuery FromQueryParams(GetAllBrandsRequest request) => new()
    {
        Active = request.Active,
        IncludeInactive = request.IncludeInactive,
        PageNumber = request.PageNumber,
        PageSize = request.PageSize
    };
}

public class GetAllBrandsQueryHandler(
    IBrandRepository repository,
    ILogger<GetAllBrandsQueryHandler> logger,
    ErrorFactory errorFactory) :
    IRequestHandler<GetAllBrandsQuery, Result<PagedResult<BrandResponse>>>
{
    public async Task<Result<PagedResult<BrandResponse>>> Handle(GetAllBrandsQuery request,
                                                                 CancellationToken cancellationToken)
    {
        try
        {
            (IReadOnlyList<Brand> items, int itemCount) =
                await repository.GetAllBrandsAsync(request.Active, request.IncludeInactive, request.PageNumber,
                                                   request.PageSize, cancellationToken);

            IReadOnlyList<BrandResponse> dtoList = items.Select(BrandResponse.FromEntity).ToList();

            PagedResult<BrandResponse> result = new PagedResult<BrandResponse>()
            {
                Items = dtoList,
                TotalCount = itemCount,
                PageSize = request.PageSize,
                PageNumber = request.PageNumber
            };

            return Result<PagedResult<BrandResponse>>.Success(result);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Data fetch operation canceled.");

            return Result<PagedResult<BrandResponse>>.Failure(errorFactory.Create(new OperationCanceled()));
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);

            return Result<PagedResult<BrandResponse>>.Failure(errorFactory.Create(new InternalError()));
        }
    }
}