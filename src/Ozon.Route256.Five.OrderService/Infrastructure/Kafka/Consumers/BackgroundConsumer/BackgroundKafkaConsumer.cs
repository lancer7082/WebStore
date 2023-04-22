using Confluent.Kafka;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Settings;
using Ozon.Route256.Five.OrderService.Infrastructure.Metrics;
using System.Diagnostics;
using System.Threading;
using static Confluent.Kafka.ConfigPropertyNames;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Consumers.BackgroundConsumer;

public class BackgroundKafkaConsumer<TKey, TMessage, THandler>: BackgroundService, IDisposable
    where THandler : IKafkaConsumerHandler<TKey, TMessage>
{
    private readonly ILogger<BackgroundKafkaConsumer<TKey, TMessage, THandler>> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ConsumerConfig _config;
    private readonly IDeserializer<TKey> _keyDeserializer;
    private readonly IDeserializer<TMessage> _messageDeserializer;
    private readonly string _topic;
    private readonly TimeSpan _timeoutForRetry;
    private readonly IKafkaMetrics _kafkaMetrics;
    private readonly IConsumer<TKey, TMessage> _consumer;

    public BackgroundKafkaConsumer(
        IServiceProvider serviceProvider,
        KafkaSettings kafkaSettings,
        ConsumerSettings consumerSettings,
        IDeserializer<TKey> keyDeserializer,
        IDeserializer<TMessage> messageDeserializer)
    {
        _logger = serviceProvider.GetRequiredService<ILogger<BackgroundKafkaConsumer<TKey, TMessage, THandler>>>();
        _serviceProvider = serviceProvider;
        _keyDeserializer = keyDeserializer;
        _messageDeserializer = messageDeserializer;
        _timeoutForRetry = TimeSpan.FromSeconds(kafkaSettings.TimeoutForRetryInSeconds);

        _config = new ConsumerConfig
        {
            GroupId = kafkaSettings.GroupId,
            BootstrapServers = kafkaSettings.BootstrapServers,
            EnableAutoCommit = consumerSettings.AutoCommit
        };

        if (string.IsNullOrWhiteSpace(consumerSettings.Topic))
            throw new InfrastructureKafkaException("Topic is empty");
        _topic = consumerSettings.Topic;
        _kafkaMetrics = serviceProvider.GetRequiredService<IKafkaMetrics>();
        _consumer = new ConsumerBuilder<TKey, TMessage>(_config)
            .SetValueDeserializer(_messageDeserializer)
            .SetKeyDeserializer(_keyDeserializer)
            .Build();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {            
        new Thread(() => StartConsumerLoop(stoppingToken)).Start();
        return Task.CompletedTask;
    }

    private void StartConsumerLoop(CancellationToken cancellationToken)
    {
        _consumer.Subscribe(_topic);
        _logger.LogInformation("Success subscribe to {Topic}", _topic);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var consumed = _consumer.Consume(cancellationToken);

                Handle(consumed.Message.Key, consumed.Message.Value, cancellationToken);

                _consumer.Commit();
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("The Kafka consumer thread has been cancelled");
                break;
            }
            catch (ConsumeException ce)
            {
                _logger.LogError($"Consume error: {ce.Error.Reason}");

                if (ce.Error.IsFatal)
                {
                    // https://github.com/edenhill/librdkafka/blob/master/INTRODUCTION.md#fatal-consumer-errors
                    _logger.LogError(ce, ce.Message);
                    break;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in topic {Topic} during kafka consume", _topic);
                Task.Delay(_timeoutForRetry, cancellationToken);
            }
        }
    }

    private void Handle(TKey key, TMessage message, CancellationToken token)
    {
        using var activity = OrderActivitySourceConfig.OrderActivitySource
            .StartActivity(typeof(THandler).Name);

        if (activity != null)
        {
            activity.AddTag("Now", DateTime.UtcNow);
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _serviceProvider.CreateScope().ServiceProvider
                .GetRequiredService<THandler>()
                .Handle(key, message, token);

            stopwatch.Stop();

            _kafkaMetrics.ResponseTime(stopwatch.ElapsedMilliseconds, typeof(THandler).Name, false);
        }
        catch
        {
            _kafkaMetrics.ResponseTime(stopwatch.ElapsedMilliseconds, typeof(THandler).Name, true);

            throw;
        }
    }

    public override void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();

        base.Dispose();
    }
}