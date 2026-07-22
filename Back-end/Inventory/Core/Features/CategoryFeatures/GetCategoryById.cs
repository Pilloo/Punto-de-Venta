using DTOs.Inventory.Category;
using ErrorHandling;
using ErrorHandling.Service;
using Inventory.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Models;

namespace Inventory.Core.Features.CategoryFeatures;

/// <summary>
/// Represents a query to retrieve a category entity by its unique identifier.
/// </summary>
/// <remarks>
/// This query encapsulates the identifier of the category to be retrieved and is processed by the corresponding handler,
/// which interacts with the repository to fetch the category data. The result indicates success or failure, and in case of
/// success, returns the requested category data.
/// </remarks>
/// <seealso cref="Category"/>
/// <seealso cref="Result{T}"/>
public record GetCategoryByIdQuery : IRequest<Result<CategoryResponse>>
{
    public Guid Id { get; init; } = Guid.Empty;

    public static GetCategoryByIdQuery FromQueryParams(Guid id) => new()
    {
        Id = id
    };
}

public class GetCategoryByIdQueryHandler(
    ICategoryRepository repository,
    ILogger<GetCategoryByIdQueryHandler> logger,
    ErrorFactory errorFactory)
    : IRequestHandler<GetCategoryByIdQuery, Result<CategoryResponse>>
{
    public async Task<Result<CategoryResponse>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            Category? category = await repository.GetCategoryAsync(request.Id, true, cancellationToken);
            
            if (category == null)
            {
                return Result<CategoryResponse>.Failure(errorFactory.Create(new NotFound()));
            }

            CategoryResponse response = CategoryResponse.FromEntity(category);
            
            return Result<CategoryResponse>.Success(response);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);

            return Result<CategoryResponse>.Failure(errorFactory.Create(new InternalError()));
        }
    }
}