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

    private readonly IConsumerDAL _consumerDal;

    public MessageConsumer(IConsumerDAL consumerDal)
    {
        _consumerDal = consumerDal ?? throw new ArgumentNullException(nameof(consumerDal));

        _consumerDal.EnsureTablesExist();
    }

    public async Task StartListening(CancellationToken cancellationToken)
    {
        Thread.Sleep(5000);

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
                            var consumeResult = consumer.Consume(TimeSpan.FromMilliseconds(100));

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

        var providers = await _consumerDal.LoadAllProviders();

        // Assign the job to providers (e.g., send notifications, create bids, etc.)
        foreach (var provider in providers)
        {
            await AssignJobToProvider(provider, newRequest);
        }
    }

    private async Task ProcessRequestUpdate(string value)
    {
        var updateRequest = JsonSerializer.Deserialize<RequestUpdated>(value);

        if(updateRequest == null)
        {
            Console.WriteLine("Request: " + value + " could not be processed/deserialized.");
        }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        switch (updateRequest.NewStatus)
        {
            case Status.Accepted:
                await _consumerDal.ProcessAcceptedRequest(updateRequest);
                break;
            case Status.Closed:
                await _consumerDal.ProcessClosedRequest(updateRequest);
                break;
            default:
                Console.WriteLine("Status: " + updateRequest.NewStatus + "Cannot be processed");
                break;
        }
#pragma warning restore CS8602 // Dereference of a possibly null reference.

        return;
    }

    private async Task AssignJobToProvider(Provider provider, RequestCreated newRequest)
    {
        if(provider.Categories?.Contains(newRequest.Category) ?? false)
        {
            var association = new ProviderRequestAssociation
            {
                Title = newRequest.Title,
                Description = newRequest.Description,
                Status = (byte)Status.Open,
                ProviderId = provider.Id,
                RequestId = newRequest.Id,
            };

            await _consumerDal.AddProviderRequestAssociation(association);

            Console.WriteLine($"Provider {provider.Name} is now bidding on the job: {JsonSerializer.Serialize(newRequest)}");
        }
    }
}