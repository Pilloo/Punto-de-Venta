using DTOs.Inventory.Product;
using ErrorHandling;
using ErrorHandling.Service;
using Inventory.Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models.Inventory;

namespace Inventory.Core.Features.ProductFeatures;

/// <summary>
/// A query object used to retrieve a product by its unique identifier.
/// </summary>
/// <remarks>
/// This query is sent to a handler that interacts with the product repository to fetch the product details.
/// The result returned includes either the product information encapsulated in a <see cref="ProductResponse"/> object
/// or an error encapsulated in a <see cref="ProblemDetails"/> object.
/// </remarks>
/// <example>
/// This query is typically used in API endpoint implementations to fetch specific product details by ID.
/// </example>
public class GetProductByIdQuery : IRequest<Result<ProductResponse>>
{
    public Guid Id { get; private init; } = Guid.Empty;

    public static GetProductByIdQuery FromQueryParams(Guid id) => new()
    {
        Id = id
    };
}

public class GetProductByIdQueryHandler(
    IProductRepository repository,
    ErrorFactory errorFactory,
    ILogger<GetProductByIdQueryHandler> logger) : IRequestHandler<GetProductByIdQuery, Result<ProductResponse>>
{
    public async Task<Result<ProductResponse>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            Product? product = await repository.GetProductAsync(request.Id, true, true, cancellationToken);

            if (product == null)
            {
                return Result<ProductResponse>.Failure(errorFactory.Create(new NotFound()));
            }

            ProductResponse response = ProductResponse.FromEntity(product);

            return Result<ProductResponse>.Success(response);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Data fetch operation canceled.");

            return Result<ProductResponse>.Failure(errorFactory.Create(new OperationCanceled()));
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);

            return Result<ProductResponse>.Failure(errorFactory.Create(new InternalError()));
        }
    }
}