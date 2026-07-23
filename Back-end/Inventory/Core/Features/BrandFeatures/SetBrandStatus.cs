using DTOs.Inventory.Brand;
using ErrorHandling;
using ErrorHandling.Service;
using Inventory.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Models.Inventory;

namespace Inventory.Core.Features.BrandFeatures;

/// <summary>
/// A command encapsulating the operation to set the status of a brand in the inventory system.
/// </summary>
/// <remarks>
/// This command is part of the Inventory.Core application layer and is designed to be handled
/// by an implementation of <see cref="IRequestHandler{TRequest, TResponse}"/>. It modifies
/// the status (e.g. active or inactive) of a brand identified by its unique identifier.
/// </remarks>
/// <example>
/// Use this command to change the status of a brand by supplying the brand's identifier and
/// the desired status (active or inactive). Typically, this command is created from a DTO
/// of type <see cref="SetBrandStatusRequest"/>.
/// </example>
/// <seealso cref="Result{T}"/>
/// <seealso cref="IBrandRepository"/>
public record SetBrandStatusCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; private init; } = Guid.Empty;

    public bool Status { get; private init; }

    public static SetBrandStatusCommand FromDto(Guid id, SetBrandStatusRequest request) => new()
    {
        Id = id,
        Status = request.Status
    };
}

public class SetBrandStatusCommandHandler(
    IBrandRepository repository,
    ILogger<SetBrandStatusCommandHandler> logger,
    ErrorFactory errorFactory)
    : IRequestHandler<SetBrandStatusCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(SetBrandStatusCommand command, CancellationToken cancellationToken)
    {
        try
        {
            Brand? brand = await repository.GetBrandAsync(command.Id, false, cancellationToken);

            if (brand == null)
            {
                return Result<Unit>.Failure(errorFactory.Create(new NotFound()));
            }

            brand.IsActive = command.Status;

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