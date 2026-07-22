using DTOs.Inventory;
using DTOs.Inventory.Brand;
using EntityFramework.Exceptions.Common;
using ErrorHandling;
using ErrorHandling.Service;
using Inventory.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Inventory.Core.Features.BrandFeatures;

/// <summary>
/// Represents the creation of a brand in the inventory system.
/// </summary>
/// <remarks>
/// This command is responsible for encapsulating the data required to create a new brand.
/// It includes the brand name and provides a factory method for instantiating the command from a DTO.
/// </remarks>
public record CreateBrandCommand : IRequest<Result<Unit>>
{
    public string Name = string.Empty;

    public static CreateBrandCommand FromDto(CreateBrandRequest request) => new()
    {
        Name = request.Name.Trim()
    };
}

public class CreateBrandHandler(
    ErrorFactory errorFactory,
    IBrandRepository repository,
    ILogger<CreateBrandHandler> logger) : IRequestHandler<CreateBrandCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(CreateBrandCommand command, CancellationToken cancellationToken)
    {
        try
        {
            Models.Brand brand = new()
            {
                Name = command.Name
            };

            await repository.CreateBrandAsync(brand, cancellationToken);

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
            logger.LogInformation("Insert operation cancelled for Product entity.");

            return Result<Unit>.Failure(errorFactory.Create(new OperationCanceled()));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);

            return Result<Unit>.Failure(errorFactory.Create(new InternalError()));
        }
    }
}