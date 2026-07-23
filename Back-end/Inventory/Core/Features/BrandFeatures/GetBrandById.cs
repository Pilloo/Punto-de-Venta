using ErrorHandling;
using ErrorHandling.Service;
using Inventory.Core.Features.ColourFeatures;
using Inventory.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Models.Inventory;

namespace Inventory.Core.Features.BrandFeatures;

/// <summary>
/// Represents a query to retrieve a brand entity by its unique identifier.
/// </summary>
/// <remarks>
/// This query encapsulates the identifier of the brand to be retrieved and is processed by the corresponding handler,
/// which interacts with the repository to fetch the brand data. The result indicates success or failure, and in case of
/// success, returns the requested brand data.
/// </remarks>
/// <seealso cref="Brand"/>
/// <seealso cref="Result{T}"/>
public record GetBrandByIdQuery : IRequest<Result<Brand?>>
{
    public Guid Id { get; init; } = Guid.Empty;
}

public class GetBrandByIdQueryHandler(
    IBrandRepository repository,
    ILogger<GetColourByIdQueryHandler> logger,
    ErrorFactory errorFactory)
    : IRequestHandler<GetBrandByIdQuery, Result<Brand?>>
{
    public async Task<Result<Brand?>> Handle(GetBrandByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            Brand? brand = await repository.GetBrandAsync(request.Id, true, cancellationToken);

            if (brand == null)
            {
                return Result<Brand?>.Failure(errorFactory.Create(new NotFound()));
            }

            return Result<Brand?>.Success(brand);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);

            return Result<Brand?>.Failure(errorFactory.Create(new InternalError()));
        }
    }
}