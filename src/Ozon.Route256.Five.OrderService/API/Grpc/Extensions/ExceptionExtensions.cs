using Grpc.Core;
using Ozon.Route256.Five.OrderService.Domain.Exceptions;

namespace Ozon.Route256.Five.OrderService.API.Grpc.Extensions;

public static class ExceptionExtensions
{
    public static RpcException Handle<T>(this Exception exception, ServerCallContext context, ILogger<T> logger) =>
        exception switch
        {
            NotFoundException => HandleNotFoundException(exception, logger),
            RpcException => HandleRpcException((RpcException)exception, logger),
            _ => HandleDefault(exception, context, logger)
        };

    private static RpcException HandleNotFoundException<T>(Exception exception, ILogger<T> logger)
    {
        //logger.LogError(exception, exception.Message);
        return new RpcException(new Status(StatusCode.NotFound, exception.Message));
    }
    private static RpcException HandleRpcException<T>(RpcException exception, ILogger<T> logger)
    {
        //logger.LogError(exception, "An error occurred");
        return new RpcException(new Status(exception.StatusCode, exception.Message));
    }

    private static RpcException HandleDefault<T>(Exception exception, ServerCallContext context, ILogger<T> logger)
    {
        //logger.LogError(exception, "An error occurred");
        return new RpcException(new Status(StatusCode.Internal, exception.Message));
    }
}
