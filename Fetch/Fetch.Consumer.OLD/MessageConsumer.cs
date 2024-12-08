using Confluent.Kafka;
public class MessageConsumer
{
    private const string KafkaBootstrapServers = "localhost:9092";
    private const string Topic = "requests";
    private const int MaxRetryAttempts = 3;
    private const int RetryDelaySeconds = 5;

    private readonly MyDbContext _context;

    public MessageConsumer(MyDbContext context)
    {
        _context = context;
    }

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

        // Query the database for available providers
        var providers = await _context.Providers.ToListAsync();

        // Assign the job to providers (e.g., send notifications, create bids, etc.)
        foreach (var provider in providers)
        {
            // For example, send an email or trigger some action
            Console.WriteLine($"Assigning job to provider {provider.Name}");
            await AssignJobToProvider(provider, message);
        }
    }

    private async Task AssignJobToProvider(Provider provider, string message)
    {
        // Logic to assign job to the provider (e.g., create a bid or send a notification)
        Console.WriteLine($"Provider {provider.Name} is now bidding on the job: {message}");
        // Simulate processing delay
        await Task.Delay(1000);
    }
}