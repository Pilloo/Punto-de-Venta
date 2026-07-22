using DTOs.Inventory.Colour;
using ErrorHandling;
using ErrorHandling.Service;
using Inventory.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Models;

namespace Inventory.Core.Features.ColourFeatures;


public record GetColourByIdQuery : IRequest<Result<ColourResponse>>
{
    public Guid Id { get; init; } = Guid.Empty;
    
    public static GetColourByIdQuery FromQueryParams(Guid id) => new()
    {
        Id = id
    };
}

public class GetColourByIdQueryHandler(
    IColourRepository repository,
    ILogger<GetColourByIdQueryHandler> logger,
    ErrorFactory errorFactory)
    : IRequestHandler<GetColourByIdQuery, Result<ColourResponse>>
{
    public async Task<Result<ColourResponse>> Handle(GetColourByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            Colour? colour = await repository.GetColourAsync(request.Id, true, cancellationToken);

            if (colour == null)
            {
                return Result<ColourResponse>.Failure(errorFactory.Create(new NotFound()));
            }

            ColourResponse response = ColourResponse.FromEntity(colour);
            
            return Result<ColourResponse>.Success(response);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);

            return Result<ColourResponse>.Failure(errorFactory.Create(new InternalError()));
        }
    }
}