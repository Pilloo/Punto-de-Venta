using DTOs.Inventory.category;
using ErrorHandling;
using ErrorHandling.Service;
using Inventory.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Models.Inventory;

namespace Inventory.Core.Features.CategoryFeatures;

/// <summary>
/// Represents a command to set the status of a category within the inventory system.
/// </summary>
/// <remarks>
/// This command is used to toggle the availability or active state of a specific category
/// using its unique identifier. It encapsulates the necessary data required to perform
/// the status update operation.
/// </remarks>
public record SetCategoryStatusCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; private init; } = Guid.Empty;

    public bool Status { get; private init; }

    public static SetCategoryStatusCommand FromDto(Guid id, SetCategoryStatusRequest request) => new()
    {
        Id = id,
        Status = request.Status
    };
}

public class SetCategoryStatusCommandHandler(
    ICategoryRepository repository,
    ILogger<SetCategoryStatusCommandHandler> logger,
    ErrorFactory errorFactory)
    : IRequestHandler<SetCategoryStatusCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(SetCategoryStatusCommand command, CancellationToken cancellationToken)
    {
        try
        {
            Category? category = await repository.GetCategoryAsync(command.Id, false, cancellationToken);

            if (category == null)
            {
                return Result<Unit>.Failure(errorFactory.Create(new NotFound()));
            }

            category.IsActive = command.Status;

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