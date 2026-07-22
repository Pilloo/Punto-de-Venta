using DTOs;
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
/// Represents a query to retrieve a paginated list of colours from the inventory.
/// </summary>
/// <remarks>
/// This query supports optional filters, including retrieving only active colours
/// or including both active and inactive colours. It also supports pagination
/// with configurable page number and page size.
/// </remarks>
public record GetAllColoursQuery : IRequest<Result<PagedResult<ColourResponse>>>
{
    public bool? Active { get; private init; }
    public bool? IncludeInactive { get; private init; }
    public int PageNumber { get; private init; } = 1;
    public int PageSize { get; private init; } = 10;

    public static GetAllColoursQuery FromQueryParams(GetAllColoursRequest request) => new()
    {
        PageNumber = request.PageNumber,
        PageSize = request.PageSize,
        Active = request.Active,
        IncludeInactive = request.IncludeInactive
    };
}

public class GetAllColoursQueryHandler(
    IColourRepository repository,
    ILogger<GetAllColoursQueryHandler> logger,
    ErrorFactory errorFactory) :
    IRequestHandler<GetAllColoursQuery, Result<PagedResult<ColourResponse>>>
{
    public async Task<Result<PagedResult<ColourResponse>>> Handle(GetAllColoursQuery request,
                                                                  CancellationToken cancellationToken)
    {
        try
        {
            (IReadOnlyCollection<Colour> items, int itemCount) =
                await repository.GetAllColoursAsync(request.Active, request.IncludeInactive, request.PageNumber,
                                                    request.PageSize, cancellationToken);

            IReadOnlyList<ColourResponse> colourList = items.Select(ColourResponse.FromEntity).ToList();

            PagedResult<ColourResponse> result = new PagedResult<ColourResponse>()
            {
                Items = colourList,
                TotalCount = itemCount,
                PageSize = request.PageSize,
                PageNumber = request.PageNumber
            };

            return Result<PagedResult<ColourResponse>>.Success(result);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Data fetch operation canceled.");

            return Result<PagedResult<ColourResponse>>.Failure(errorFactory.Create(new OperationCanceled()));
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);

            return Result<PagedResult<ColourResponse>>.Failure(errorFactory.Create(new InternalError()));
        }
    }
}