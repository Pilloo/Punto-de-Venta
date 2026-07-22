using DTOs.Inventory;
using DTOs.Inventory.Colour;
using ErrorHandling;
using ErrorHandling.Service;
using Inventory.Core.Features.BrandFeatures;
using Inventory.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Models;

namespace Inventory.Core.Features.ColourFeatures;

/// <summary>
/// Represents a command to modify the details of an existing colour in the system.
/// </summary>
/// <remarks>
/// <para>
/// This command is used to update the information of a colour such as its name using the colour's unique identifier.
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
public record ModifyColourCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;

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