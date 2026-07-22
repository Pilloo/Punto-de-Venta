using System.ComponentModel.DataAnnotations;
using DTOs.Inventory;
using DTOs.Inventory.Colour;
using ErrorHandling;
using ErrorHandling.Service;
using Inventory.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Models;

namespace Inventory.Core.Features.ColourFeatures;

/// <summary>
/// A command encapsulating the operation to set the status of a colour in the inventory system.
/// </summary>
/// <remarks>
/// This command is part of the Inventory.Core application layer and is designed to be handled
/// by an implementation of <see cref="IRequestHandler{TRequest, TResponse}"/>. It modifies
/// the status (e.g. active or inactive) of a colour identified by its unique identifier.
/// </remarks>
/// <example>
/// Use this command to change the status of a colour by supplying the colour's identifier and
/// the desired status (active or inactive). Typically, this command is created from a DTO
/// of type <see cref="SetColourStatusRequest"/>.
/// </example>
/// <seealso cref="Result{T}"/>
/// <seealso cref="IColourRepository"/>
public class SetColourStatusCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; set; } = Guid.Empty;

    public bool Status { get; set; } = false;

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