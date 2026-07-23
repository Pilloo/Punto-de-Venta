using DTOs;
using DTOs.Inventory.Category;
using ErrorHandling;
using ErrorHandling.Service;
using Inventory.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Models.Inventory;

namespace Inventory.Core.Features.CategoryFeatures;

/// <summary>
/// Represents a query to retrieve a paginated list of categories from the inventory system.
/// This query supports filtering based on the status of the categories and pagination options.
/// </summary>
/// <remarks>
/// The query can include both active and inactive categories, depending on the specified filtering parameters.
/// Pagination is managed through the PageNumber and PageSize properties.
/// The result is a paginated list of categories wrapped in a <see cref="Result{T}"/> object.
/// Use the <c>FromQueryParams</c> method to create an instance of this query from a
/// <see cref="GetAllCategoriesRequest"/> object.
/// </remarks>
public record GetAllCategoriesQuery : IRequest<Result<PagedResult<CategoryResponse>>>
{
    public bool? Active { get; private init; }
    public bool? IncludeInactive { get; private init; }
    public int PageNumber { get; private init; } = 1;
    public int PageSize { get; private init; } = 10;

    public static GetAllCategoriesQuery FromQueryParams(GetAllCategoriesRequest request) => new()
    {
        Active = request.Active,
        IncludeInactive = request.IncludeInactive,
        PageNumber = request.PageNumber,
        PageSize = request.PageSize
    };
}

public class GetAllCategoriesQueryHandler(
    ICategoryRepository repository,
    ILogger<GetAllCategoriesQueryHandler> logger,
    ErrorFactory errorFactory) :
    IRequestHandler<GetAllCategoriesQuery, Result<PagedResult<CategoryResponse>>>
{
    public async Task<Result<PagedResult<CategoryResponse>>> Handle(GetAllCategoriesQuery request,
                                                                    CancellationToken cancellationToken)
    {
        try
        {
            (IReadOnlyCollection<Category> items, int totalCount) =
                await repository.GetAllCategoriesAsync(request.Active, request.IncludeInactive, request.PageNumber,
                                                       request.PageSize, cancellationToken);

            IReadOnlyList<CategoryResponse> dtoList = items.Select(CategoryResponse.FromEntity).ToList();

            PagedResult<CategoryResponse> result = new PagedResult<CategoryResponse>()
            {
                Items = dtoList,
                TotalCount = totalCount,
                PageSize = request.PageSize,
                PageNumber = request.PageNumber
            };

            return Result<PagedResult<CategoryResponse>>.Success(result);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Data fetch operation canceled.");

            return Result<PagedResult<CategoryResponse>>.Failure(errorFactory.Create(new OperationCanceled()));
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);

            return Result<PagedResult<CategoryResponse>>.Failure(errorFactory.Create(new InternalError()));
        }
    }
}