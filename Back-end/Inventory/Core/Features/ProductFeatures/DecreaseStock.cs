using ErrorHandling;
using ErrorHandling.Service;
using Inventory.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Models;

namespace Inventory.Core.Features.ProductFeatures;

public record DecreaseStockCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; init; } = Guid.Empty;
    public int Quantity { get; init; }

    public static DecreaseStockCommand FromRequest(Guid id, int quantity) => new()
    {
        Id = id,
        Quantity = quantity
    };
}

public class DecreaseStockCommandHandler(
    IProductRepository repository,
    ILogger<DecreaseStockCommandHandler> logger,
    ErrorFactory errorFactory) : IRequestHandler<DecreaseStockCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DecreaseStockCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Product? entity = await repository.GetProductAsync(request.Id, false, false, cancellationToken);

            if (entity == null)
            {
                return Result<Unit>.Failure(errorFactory.Create(new NotFound()));
            }

            if (entity.StockCount < request.Quantity)
            {
                return Result<Unit>.Failure(errorFactory.Create(new InsufficientStock(request.Quantity)));
            }

            entity.StockCount -= request.Quantity;

            await repository.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Update operation cancelled for Product entity.");
            
            return Result<Unit>.Failure(errorFactory.Create(new OperationCanceled()));
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            
            return Result<Unit>.Failure(errorFactory.Create(new InternalError()));
        }
    }
}