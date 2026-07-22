using DTOs.Inventory.Product;
using EntityFramework.Exceptions.Common;
using ErrorHandling;
using ErrorHandling.Service;
using Inventory.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Models;

namespace Inventory.Core.Features.ProductFeatures;

/// <summary>
/// Represents a command to create a new product in the inventory system.
/// </summary>
/// <remarks>
/// This command is designed to encapsulate the necessary data required to
/// add a new product to the inventory, including brand, colour, category,
/// item summary, barcode details, stock levels, and pricing information.
/// </remarks>
public class CreateProductCommand : IRequest<Result<Unit>>
{
    public Guid? BrandId { get; set; } = Guid.Empty;
    public Guid? CategoryId { get; set; } = Guid.Empty;
    public Guid? ColourId { get; set; } = Guid.Empty;

    public string ItemSummary { get; set; } = string.Empty;

    public string BarcodeContent { get; set; } = string.Empty;

    public int StockCount { get; set; } = 0;
    public int MinimumStockLevel { get; set; } = 0;

    public decimal Price { get; set; } = 0;

    public static CreateProductCommand FromDto(CreateProductRequest request) => new()
    {
        BrandId = request.BrandId ?? null,
        CategoryId = request.CategoryId ?? null,
        ColourId = request.ColourId ?? null,
        ItemSummary = request.ItemSummary,
        BarcodeContent = request.BarcodeContent,
        StockCount = request.StockCount,
        MinimumStockLevel = request.MinimumStockLevel,
        Price = request.Price
    };
}

public class CreateProductCommandHandler(
    IProductRepository repository,
    ErrorFactory errorFactory,
    ILogger<CreateProductCommandHandler> logger)
    : IRequestHandler<CreateProductCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        try
        {
            (IReadOnlyCollection<Product> searchResult, int totalCount) = await repository.GetProductsAsync(
                x => x.BarcodeContent.Contains(command.BarcodeContent), true, false, null, null, cancellationToken);

            if (totalCount > 0)
            {
                return Result<Unit>.Failure(errorFactory.Create(new DuplicatedEntity()));
            }

            Product product = new()
            {
                BrandId = command.BrandId,
                CategoryId = command.CategoryId,
                ColourId = command.ColourId,
                ItemSummary = command.ItemSummary,
                BarcodeContent = command.BarcodeContent,
                StockCount = command.StockCount,
                MinimumStockLevel = command.MinimumStockLevel,
                Price = command.Price,
                IsActive = true
            };

            await repository.CreateProductAsync(product, cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (ReferenceConstraintException e)
        {
            logger.LogWarning(e, "Constraint violation detected. Constraint violated: {constraint}",
                              e.ConstraintName);

            return Result<Unit>.Failure(errorFactory.Create(new ConstraintViolationException(e.ConstraintName)));
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Insert operation cancelled for Product entity.");

            return Result<Unit>.Failure(errorFactory.Create(new OperationCanceled()));
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);

            return Result<Unit>.Failure(errorFactory.Create(new InternalError()));
        }
    }
}