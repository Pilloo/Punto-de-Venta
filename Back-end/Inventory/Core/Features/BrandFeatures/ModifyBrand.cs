using DTOs.Inventory;
using DTOs.Inventory.Brand;
using ErrorHandling;
using ErrorHandling.Service;
using Inventory.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Models;

namespace Inventory.Core.Features.BrandFeatures;

/// <summary>
/// Represents a command to modify the details of an existing brand in the system.
/// </summary>
/// <remarks>
/// <para>
/// This command is used to update the information of a brand such as its name using the brand's unique identifier.
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
public record ModifyBrandCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    
    public static ModifyBrandCommand FromDto(Guid id, ModifyBrandRequest request) => new()
    {
        Id = id,
        Name = request.Name
    };
}

public class ModifyBrandHandler(
    IBrandRepository repository,
    ILogger<ModifyBrandHandler> logger,
    ErrorFactory errorFactory)
    : IRequestHandler<ModifyBrandCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(ModifyBrandCommand command, CancellationToken cancellationToken)
    {
        try
        {
            Brand? brand = await repository.GetBrandAsync(command.Id, false, cancellationToken);

            if (brand == null)
            {
                return Result<Unit>.Failure(errorFactory.Create(new NotFound()));
            }

            brand.Name = command.Name.Trim();

            await repository.UpdateBrandAsync(brand, cancellationToken);

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