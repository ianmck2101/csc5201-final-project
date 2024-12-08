public class KafkaConsumerHostedService : IHostedService
{
    private readonly MessageConsumer _consumer;
    private readonly CancellationTokenSource _cts = new();

    public KafkaConsumerHostedService(MessageConsumer consumer)
    {
        _consumer = consumer;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Start the consumer in the background
        _ = _consumer.StartListening(_cts.Token);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel(); // Signal cancellation to consumer
        return Task.CompletedTask;
    }
}
