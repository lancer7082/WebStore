using Confluent.Kafka;
using Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Consumers.BackgroundConsumer;
using Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Settings;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Consumers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConsumer<TKey, TMessage, THandler>(
        this IServiceCollection services,
        IConfiguration configuration,
        ConsumerType consumerType,
        KafkaSettings kafkaSettings,
        IDeserializer<TKey> keyDeserializer,
        IDeserializer<TMessage> valueDeserializer) where THandler : class, IKafkaConsumerHandler<TKey, TMessage>
    {
        var consumerSettings = configuration.GetSection($"Kafka:Consumer:{consumerType.Name}")
            .Get<ConsumerSettings>();

        if (!consumerSettings.Enabled)
            return services;

        services.AddHostedService<BackgroundKafkaConsumer<TKey, TMessage, THandler>>(sp =>
            new BackgroundKafkaConsumer<TKey, TMessage, THandler>(
                sp,
                kafkaSettings,
                consumerSettings,
                keyDeserializer,
                valueDeserializer)
            );
        services.AddScoped<THandler>();
        return services;
    }
}