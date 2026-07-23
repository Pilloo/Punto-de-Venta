using DTOs.Inventory.Category;
using EntityFramework.Exceptions.Common;
using ErrorHandling;
using ErrorHandling.Service;
using Inventory.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Models.Inventory;

namespace Inventory.Core.Features.CategoryFeatures;

/// <summary>
/// Represents the creation of a category in the inventory system.
/// </summary>
/// <remarks>
/// This command is responsible for encapsulating the data required to create a new category.
/// It includes the category name and provides a factory method for instantiating the command from a DTO.
/// </remarks>
public record CreateCategoryCommand : IRequest<Result<Unit>>
{
    public string Name = string.Empty;

    public static CreateCategoryCommand FromDto(CreateCategoryRequest request) => new()
    {
        Name = request.Name.Trim()
    };
}

public class CreateCategoryHandler(
    ErrorFactory errorFactory,
    ICategoryRepository repository,
    ILogger<CreateCategoryHandler> logger) : IRequestHandler<CreateCategoryCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        try
        {
            Category category = new()
            {
                Name = command.Name
            };

            await repository.CreateCategoryAsync(category, cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (UniqueConstraintException e)
        {
            logger.LogInformation(e, "Duplicated entity insertion detected. Constraint violated: {constraint}",
                                  e.ConstraintName);

            return Result<Unit>.Failure(errorFactory.Create(new DuplicatedEntity()));
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Insert operation cancelled for Colour entity.");

            return Result<Unit>.Failure(errorFactory.Create(new OperationCanceled()));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);

            return Result<Unit>.Failure(errorFactory.Create(new InternalError()));
        }
    }
}