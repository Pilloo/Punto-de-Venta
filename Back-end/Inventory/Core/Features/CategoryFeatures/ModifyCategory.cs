using DTOs.Inventory;
using DTOs.Inventory.Category;
using DTOs.Inventory.Colour;
using ErrorHandling;
using ErrorHandling.Service;
using Inventory.Core.Features.BrandFeatures;
using Inventory.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Models;

namespace Inventory.Core.Features.CategoryFeatures;

/// <summary>
/// Represents a command to modify the details of an existing category in the system.
/// </summary>
/// <remarks>
/// <para>
/// This command is used to update the information of a category such as its name using the category's unique identifier.
/// </para>
/// <para>
/// The command follows the CQRS (Command Query Responsibility Segregation) pattern and is processed by
/// a dedicated handler.
/// </para>
/// </remarks>
/// <example>
/// The process of handling this command can result in either a successful execution or a failure with
/// structured error details provided by the <see cref="Result"/> class.
/// </example>
public record ModifyCategoryCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;

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