using ErrorHandling;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Inventory.Core.Features.ProductFeatures;
using MediatR;
using ProductsService.Protos;
using static ProductsService.Protos.ProductsService;

namespace Inventory.Presentation.Grpc;

public class ProductsGrpcService(IMediator mediator, ILogger<ProductsGrpcService> logger) : ProductsServiceBase
{
    public override async Task<Empty> DecreaseStock(DecreaseStockRequest request, ServerCallContext context)
    {
        try
        {
            DecreaseStockCommand command =
                DecreaseStockCommand.FromRequest(Guid.Parse(request.ProductId), request.Quantity);

            Result<Unit> result = await mediator.Send(command);

            if (!result.IsSuccess)
            {
                throw result.Error!.Status switch
                {
                    (int)HttpCodes.NotFound => new RpcException(new Status(StatusCode.NotFound, result.Error.Detail!)),
                    (int)HttpCodes.BadRequest => new RpcException(
                        new Status(StatusCode.ResourceExhausted, result.Error.Detail!)),
                    _ => new RpcException(new Status(StatusCode.Unknown, result.Error.Detail!))
                };
            }

            return new Empty();
        }
        catch (RpcException e)
        {
            logger.LogDebug(e, e.Message);

            throw;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);

            throw new RpcException(new Status(StatusCode.Internal, "An unexpected error occurred."));
        }
    }
}