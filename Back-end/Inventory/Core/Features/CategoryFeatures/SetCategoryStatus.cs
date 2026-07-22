using System.ComponentModel.DataAnnotations;
using DTOs.Inventory;
using DTOs.Inventory.category;
using DTOs.Inventory.Colour;
using ErrorHandling;
using ErrorHandling.Service;
using Inventory.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Models;

namespace Inventory.Core.Features.CategoryFeatures;

/// <summary>
/// A command encapsulating the operation to set the status of a category in the inventory system.
/// </summary>
/// <remarks>
/// This command is part of the Inventory.Core application layer and is designed to be handled
/// by an implementation of <see cref="IRequestHandler{TRequest, TResponse}"/>. It modifies
/// the status (e.g. active or inactive) of a category identified by its unique identifier.
/// </remarks>
/// <example>
/// Use this command to change the status of a category by supplying the category's identifier and
/// the desired status (active or inactive). Typically, this command is created from a DTO
/// of type <see cref="SetCategoryStatusRequest"/>.
/// </example>
/// <seealso cref="Result{T}"/>
/// <seealso cref="ICategoryRepository"/>
public class SetCategoryStatusCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; set; } = Guid.Empty;

    public bool Status { get; set; } = false;

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