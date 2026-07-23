using DTOs.Inventory.Product;
using ErrorHandling;
using ErrorHandling.Service;
using Inventory.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Models.Inventory;

namespace Inventory.Core.Features.ProductFeatures;

/// <summary>
/// Represents the command used to modify the properties of an existing product in the inventory.
/// </summary>
/// <remarks>
/// This command is part of the application layer and is used to capture and transfer the updated product details.
/// It implements the <see cref="IRequest{TResponse}"/> interface from MediatR, encapsulating the data required
/// to process a product modification operation within the system.
/// </remarks>
/// <seealso cref="ModifyProductRequest"/>
/// <seealso cref="IRequestHandler{TRequest,TResponse}"/>
public class ModifyProductCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; private init; }

    public Guid? BrandId { get; private init; }
    public Guid? ColourId { get; private init; }
    public Guid? CategoryId { get; private init; }

    public string? ItemSummary { get; private init; }

    public string? BarcodeContent { get; private init; }

    public int? StockCount { get; private init; }
    public int? MinimumStockLevel { get; private init; }

    public decimal? Price { get; private init; }

    public static ModifyProductCommand FromDto(ModifyProductRequest request) => new()
    {
        Id = request.Id,
        BrandId = request.BrandId ?? null,
        ColourId = request.ColourId ?? null,
        CategoryId = request.CategoryId ?? null,
        ItemSummary = request.ItemSummary,
        BarcodeContent = request.BarcodeContent,
        StockCount = request.StockCount,
        MinimumStockLevel = request.MinimumStockLevel,
        Price = request.Price
    };
}

public class ModifyProductHandler(
    IProductRepository repository,
    ILogger<ModifyProductHandler> logger,
    ErrorFactory errorFactory) : IRequestHandler<ModifyProductCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(ModifyProductCommand command, CancellationToken cancellationToken)
    {
        try
        {
            Product? entity = await repository.GetProductAsync(command.Id, false, false, cancellationToken);

            if (entity == null)
            {
                return Result<Unit>.Failure(errorFactory.Create(new NotFound()));
            }

            entity.BrandId = command.BrandId ?? entity.BrandId;
            entity.ColourId = command.ColourId ?? entity.ColourId;
            entity.CategoryId = command.CategoryId ?? entity.CategoryId;
            entity.ItemSummary = command.ItemSummary ?? entity.ItemSummary;
            entity.BarcodeContent = command.BarcodeContent ?? entity.BarcodeContent;
            entity.StockCount = command.StockCount ?? entity.StockCount;
            entity.MinimumStockLevel = command.MinimumStockLevel ?? entity.MinimumStockLevel;
            entity.Price = command.Price ?? entity.Price;

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