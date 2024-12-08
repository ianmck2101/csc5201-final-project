using Confluent.Kafka;

namespace Fetch.Consumer
{
    public class MessageConsumer
    {
        private const string KafkaBootstrapServers = "localhost:9092";
        private const string Topic = "requests";
        private const int MaxRetryAttempts = 3;
        private const int RetryDelaySeconds = 5;

        public async Task StartListening(CancellationToken cancellationToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = KafkaBootstrapServers,
                GroupId = "fetch-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            int attempt = 0;

            while (attempt < MaxRetryAttempts)
            {
                try
                {
                    attempt++;
                    Console.WriteLine($"Attempt {attempt} to connect to Kafka...");

                    using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
                    {
                        consumer.Subscribe(Topic);

                        while (!cancellationToken.IsCancellationRequested)
                        {
                            try
                            {
                                var consumeResult = consumer.Consume(TimeSpan.FromMilliseconds(1000));
                                if (consumeResult != null)
                                {
                                    Console.WriteLine($"Received message: {consumeResult.Message.Value}");
                                    await ProcessMessage(consumeResult.Message.Value);
                                }
                            }
                            catch (ConsumeException ex)
                            {
                                Console.WriteLine($"Consume error: {ex.Error.Reason}");
                            }
                        }
                    }

                    break;
                }
                catch (ConsumeException ex)
                {
                    Console.WriteLine($"Failed to connect on attempt {attempt}. Error: {ex.Error.Reason}");

                    if (attempt >= MaxRetryAttempts)
                    {
                        Console.WriteLine("Max retry attempts reached. Exiting...");
                        Environment.Exit(1);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(RetryDelaySeconds));
                }
            }
        }

        private async Task ProcessMessage(string message)
        {
            Console.WriteLine($"Processing message: {message}");
            // Simulate some processing delay
            await Task.Delay(1000);
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var consumer = new MessageConsumer();
            var cancellationTokenSource = new CancellationTokenSource();

            await consumer.StartListening(cancellationTokenSource.Token);
        }
    }
}
