using DTOs.Inventory.Product;
using ErrorHandling;
using ErrorHandling.Service;
using Inventory.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Models.Inventory;

namespace Inventory.Core.Features.ProductFeatures;

/// <summary>
/// Represents a command to update the status of a product in the inventory system.
/// </summary>
/// <remarks>
/// Use this command to modify the active state of a product. The command encapsulates
/// the product identifier and the desired status. This command is typically handled
/// by the <see cref="SetProductStatusCommandHandler"/> within the application's core business logic.
/// </remarks>
public class SetProductStatusCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; private init; }
    public bool Status { get; private init; }

    public static SetProductStatusCommand FromDto(Guid id, SetProductStatusRequest request) => new()
    {
        Id = id,
        Status = request.Status
    };
}

public class SetProductStatusCommandHandler(
    IProductRepository repository,
    ILogger<SetProductStatusCommandHandler> logger,
    ErrorFactory errorFactory) : IRequestHandler<SetProductStatusCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(SetProductStatusCommand command, CancellationToken cancellationToken)
    {
        try
        {
            Product? product = await repository.GetProductAsync(command.Id, false, false, cancellationToken);

            if (product == null)
            {
                return Result<Unit>.Failure(errorFactory.Create(new NotFound()));
            }

            product.IsActive = command.Status;

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