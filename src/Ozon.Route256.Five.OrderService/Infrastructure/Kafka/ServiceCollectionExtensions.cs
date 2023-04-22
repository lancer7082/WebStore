using Confluent.Kafka;
using Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Consumers;
using Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Consumers.BackgroundConsumer;
using Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Consumers.OrderEvents;
using Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Consumers.OrderEvents.Dto;
using Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Consumers.PreOrders;
using Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Consumers.PreOrders.Dto;
using Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Producers;
using Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Producers.NewOrders;
//using Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Producers;
//using Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Producers.OrderEvents;
using Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Settings;
using Ozon.Route256.Five.OrderService.Infrastructure.Metrics;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Kafka;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafka(this IServiceCollection services, 
        IConfiguration configuration)
    {
        var kafkaSettings = configuration.GetSection("Kafka").Get<KafkaSettings>();
        
        services.AddConsumer<byte[], PreOrderDto, PreOrdersConsumerHandler>(
            configuration,
            ConsumerType.PreOrder,
            kafkaSettings,
            Deserializers.ByteArray,
            new KafkaJsonSerializer<PreOrderDto>());

        services.AddConsumer<string, OrderEventChangedDto, OrderEventsConsumerHandler>(
            configuration,
            ConsumerType.OrderEvent,
            kafkaSettings,
            Deserializers.Utf8,
            new KafkaJsonSerializer<OrderEventChangedDto>());

        services.AddProducer(configuration, kafkaSettings);

        services.Configure<NewOrdersSettings>(configuration.GetSection(NewOrdersSettings.Sections));
        services.AddScoped<INewOrdersKafkaPublisher, NewOrdersKafkaPublisher>();

        return services;
    }
}