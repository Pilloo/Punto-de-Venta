using DTOs.Inventory.Category;
using ErrorHandling;
using ErrorHandling.Service;
using Inventory.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Models.Inventory;

namespace Inventory.Core.Features.CategoryFeatures;

/// <summary>
/// Represents a command to modify an existing category in the inventory system.
/// </summary>
/// <remarks>
/// The <c>ModifyCategoryCommand</c> contains the data required to update a category, such as its unique identifier
/// and the new name. It implements the <c>IRequest&lt;Result&lt;Unit&gt;&gt;</c> interface, enabling it to be
/// handled via MediatR for asynchronous processing.
/// </remarks>
/// <example>
/// This command may be used by a handler to update category information in the repository.
/// </example>
public record ModifyCategoryCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; private init; }
    public string Name { get; private init; } = string.Empty;

    public static ModifyCategoryCommand FromDto(Guid id, ModifyCategoryRequest request) => new()
    {
        Id = request.Id,
        Name = request.Name
    };
}

public class ModifyCategoryHandler(
    ICategoryRepository repository,
    ILogger<ModifyCategoryHandler> logger,
    ErrorFactory errorFactory)
    : IRequestHandler<ModifyCategoryCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(ModifyCategoryCommand command, CancellationToken cancellationToken)
    {
        try
        {
            Category? category = await repository.GetCategoryAsync(command.Id, false, cancellationToken);

            if (category == null)
            {
                return Result<Unit>.Failure(errorFactory.Create(new NotFound()));
            }

            category.Name = command.Name.Trim();

            await repository.UpdateCategoryAsync(category, cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Update operation cancelled for Category entity.");

            return Result<Unit>.Failure(errorFactory.Create(new OperationCanceled()));
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);

            return Result<Unit>.Failure(errorFactory.Create(new InternalError()));
        }
    }
}