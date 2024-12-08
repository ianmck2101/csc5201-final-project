using System.Text.Json;
using Confluent.Kafka;
using Fetch.Consumer;
using Fetch.Models.Data;
using Fetch.Models.Events;
public class MessageConsumer
{
    private const string KafkaBootstrapServers = "kafka:9092";
    private const string NewRequestsTopic = "requests";
    private const string UpdateRequestTopic = "update-request";
    private const int MaxRetryAttempts = 3;
    private const int RetryDelaySeconds = 5;

    private readonly IConsumerDAL _providerDal;

    public MessageConsumer(IConsumerDAL providerDal)
    {
        _providerDal = providerDal ?? throw new ArgumentNullException(nameof(providerDal));

        _providerDal.EnsureTablesExist();
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
                    consumer.Subscribe(new[] { NewRequestsTopic, UpdateRequestTopic });

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            Console.WriteLine("Polling Kafka for messages...");
                            var consumeResult = consumer.Consume(TimeSpan.FromMilliseconds(1000));

                            if (consumeResult != null && consumeResult.Message != null)
                            {
                                Console.WriteLine($"Received message from topic {consumeResult.Topic}: {consumeResult.Message.Value}");

                                if (consumeResult.Topic.Equals(NewRequestsTopic))
                                {
                                    await ProcessNewRequest(consumeResult.Message.Value);
                                }
                                else if (consumeResult.Topic.Equals(UpdateRequestTopic))
                                {
                                    await ProcessRequestUpdate(consumeResult.Message.Value);
                                }
                            }
                        }
                        catch (ConsumeException ex)
                        {
                            Console.WriteLine($"Consume error: {ex.Error.Reason}");
                        }
                        catch (Exception ex)
                        {
                            // Catch any other exceptions to prevent consumer from crashing
                            Console.WriteLine($"Unexpected error: {ex.Message}");
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

    private async Task ProcessNewRequest(string message)
    {
        Console.WriteLine($"Processing message: {message}");

        var newRequest = JsonSerializer.Deserialize<RequestCreated>(message);

        if (newRequest == null)
        {
            Console.WriteLine(message + "was null. Skipping");
            return;
        }

        var providers = await _providerDal.LoadAllProviders();

        // Assign the job to providers (e.g., send notifications, create bids, etc.)
        foreach (var provider in providers)
        {
            await AssignJobToProvider(provider, newRequest);
        }
    }

    private async Task ProcessRequestUpdate(string value)
    {
        Console.WriteLine(value + "would be processed");

        await Task.Delay(500);
        return;
    }

    private async Task AssignJobToProvider(Provider provider, RequestCreated newRequest)
    {
        if(provider.Categories?.Contains(newRequest.Category) ?? false)
        {
            var association = new ProviderRequestAssociation
            {
                Description = newRequest.Description,
                Status = (byte)Status.Open,
                ProviderId = provider.Id
            };

            await _providerDal.AddProviderRequestAssociation(association);

            Console.WriteLine($"Provider {provider.Name} is now bidding on the job: {JsonSerializer.Serialize(newRequest)}");
        }
    }
}