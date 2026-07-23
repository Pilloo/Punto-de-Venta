using DTOs.Inventory.Colour;
using EntityFramework.Exceptions.Common;
using ErrorHandling;
using ErrorHandling.Service;
using Inventory.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Models.Inventory;

namespace Inventory.Core.Features.ColourFeatures;

/// <summary>
/// Represents the creation of a colour in the inventory system.
/// </summary>
/// <remarks>
/// This command is responsible for encapsulating the data required to create a new colour.
/// It includes the brand name and provides a factory method for instantiating the command from a DTO.
/// </remarks>
public record CreateColourCommand : IRequest<Result<Unit>>
{
    public string Name { get; private init; } = string.Empty;

    public static CreateColourCommand FromDto(CreateColourRequest request) => new()
    {
        Name = request.Name.Trim()
    };
}

public class CreateColourHandler(
    ErrorFactory errorFactory,
    IColourRepository repository,
    ILogger<CreateColourHandler> logger) : IRequestHandler<CreateColourCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(CreateColourCommand command, CancellationToken cancellationToken)
    {
        try
        {
            Colour colour = new()
            {
                Name = command.Name
            };

            await repository.CreateColourAsync(colour, cancellationToken);

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