using DTOs.Inventory.Product;
using ErrorHandling;
using ErrorHandling.Service;
using Inventory.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Models;

namespace Inventory.Core.Features.ProductFeatures;

public class GetProductByIdQuery : IRequest<Result<ProductResponse>>
{
    public Guid Id { get; init; } = Guid.Empty;

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