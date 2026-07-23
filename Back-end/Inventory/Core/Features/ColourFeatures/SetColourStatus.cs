using DTOs.Inventory.Colour;
using ErrorHandling;
using ErrorHandling.Service;
using Inventory.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Models.Inventory;

namespace Inventory.Core.Features.ColourFeatures;

/// <summary>
/// Represents a command to update the status of a colour entity in the inventory system.
/// </summary>
/// <remarks>
/// This command encapsulates the necessary data to modify the status of a colour resource,
/// including the unique identifier of the target entity and the desired status.
/// </remarks>
public record SetColourStatusCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; private init; } = Guid.Empty;

    public bool Status { get; private init; }

    public static SetColourStatusCommand FromDto(Guid id, SetColourStatusRequest request) => new()
    {
        Id = id,
        Status = request.Status
    };
}

public class SetColourStatusCommandHandler(
    IColourRepository repository,
    ILogger<SetColourStatusCommandHandler> logger,
    ErrorFactory errorFactory)
    : IRequestHandler<SetColourStatusCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(SetColourStatusCommand command, CancellationToken cancellationToken)
    {
        try
        {
            Colour? colour = await repository.GetColourAsync(command.Id, false, cancellationToken);

            if (colour == null)
            {
                return Result<Unit>.Failure(errorFactory.Create(new NotFound()));
            }

            colour.IsActive = command.Status;

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