using Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Consumers.BackgroundConsumer;
using Ozon.Route256.Five.OrderService.Domain;
using Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Consumers.OrderEvents.Dto;
using Ozon.Route256.Five.OrderService.Infrastructure.DateTimeProvider;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Consumers.OrderEvents;

public class OrderEventsConsumerHandler: IKafkaConsumerHandler<string, OrderEventChangedDto>
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<OrderEventsConsumerHandler> _logger;
    private readonly IOrdersService _ordersService;

    public OrderEventsConsumerHandler(IOrdersService orderService,
        IDateTimeProvider dateTimeProvider,
        ILogger<OrderEventsConsumerHandler> logger
        )
    {
        _ordersService = orderService;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task Handle(string key, OrderEventChangedDto message, CancellationToken token)
    {
        _logger.LogInformation("Start handle event: {Name}", "OrderEventChanged");

        if (message.Id == 0)
        {
            throw new ArgumentNullException(nameof(message.Id));
        }

        var newState = message.NewState.ToOrderState();

        await _ordersService.UpdateOrderStateAsync(message.Id, newState, token);

        _logger.LogInformation("Successfully handled event: {Name} , key = {OrderId}", "OrderEventChanged", message.Id);

        return;
    }
}