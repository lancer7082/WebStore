using Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Consumers.BackgroundConsumer;
using Ozon.Route256.Five.OrderService.Domain;
using Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Consumers.PreOrders.Dto;
using Ozon.Route256.Five.OrderService.Domain.Exceptions;
using Ozon.Route256.Five.OrderService.Infrastructure.DateTimeProvider;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Consumers.PreOrders;

public class PreOrdersConsumerHandler : IKafkaConsumerHandler<byte[], PreOrderDto>
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<PreOrdersConsumerHandler> _logger;
    private readonly IOrdersService _ordersService;
    private readonly ICustomersService _customersService;

    public PreOrdersConsumerHandler(IOrdersService orderService,
        ICustomersService customerService,
        IDateTimeProvider dateTimeProvider,
        ILogger<PreOrdersConsumerHandler> logger
        )
    {
        _ordersService = orderService;
        _dateTimeProvider = dateTimeProvider;
        _customersService = customerService;
        _logger = logger;
    }

    public async Task Handle(byte[] key, PreOrderDto message, CancellationToken token)
    {
        _logger.LogInformation("Start handle event: {Name}", "PreOrder");

        if (message.Customer == null) 
        {
            throw new InvalidArgumentException(nameof(message.Customer));
        }

        if (message.Customer.Id == 0)
        {
            _logger.LogWarning("Skip customer with Id = 0"); // ? почему-то приходят такие из pre_orders
            return;
        }    

        // Получение данных клиента из сервиса CustomerService
        var customer = await _customersService.GetAsync(message.Customer.Id, token);

        var order = message.ConvertToOrderDto(customer, _dateTimeProvider.CurrentDateTimeOffsetUtc.DateTime);

        // Сохранение заказа в репозиторий
        try
        {
            await _ordersService.SaveIfNotExists(order, token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while saving order {orderId}", order.Id);
            throw;
        }

        // Валидация заказа и отправка в топик new_orders
        try
        {
            await _ordersService.CheckAndPublishNewOrderAsync(order.Id, token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while publishing new order {orderId}", order.Id);
            throw;
        }

        _logger.LogInformation("Successfully handled event: {Name} , OrderId = {OrderId}", "PreOrder", order.Id);

        return;
    }
}