using DTOs.Inventory.Colour;
using ErrorHandling;
using ErrorHandling.Service;
using Inventory.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Models.Inventory;

namespace Inventory.Core.Features.ColourFeatures;

/// <summary>
/// Represents a command to modify a colour entity in the system.
/// </summary>
/// <remarks>
/// This command is used within a CQRS pattern to update the properties of an existing colour entity.
/// It encapsulates the data required to perform the modification and is processed by an appropriate handler.
/// </remarks>
/// <example>
/// This command is typically dispatched using a mediator in order to trigger the corresponding handler logic.
/// </example>
public record ModifyColourCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; private init; }
    public string Name { get; private init; } = string.Empty;

    public static ModifyColourCommand FromDto(Guid id, ModifyColourRequest request) => new()
    {
        Id = id,
        Name = request.Name
    };
}

public class ModifyColourHandler(
    IColourRepository repository,
    ILogger<ModifyColourHandler> logger,
    ErrorFactory errorFactory)
    : IRequestHandler<ModifyColourCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(ModifyColourCommand command, CancellationToken cancellationToken)
    {
        try
        {
            Colour? colour = await repository.GetColourAsync(command.Id, false, cancellationToken);

            if (colour == null)
            {
                return Result<Unit>.Failure(errorFactory.Create(new NotFound()));
            }

            colour.Name = command.Name.Trim();

            await repository.UpdateColourAsync(colour, cancellationToken);

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