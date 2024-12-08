using Confluent.Kafka;

namespace Fetch.Api
{
    public interface IKafkaProducer
    {
        Task ProduceNewRequestMessageAsync(string message);
        Task ProduceRequestUpdatedMessageAsync(string message);
    }

    public class KafkaProducer : IKafkaProducer
    {
        private const string KafkaBootstrapServers = "kafka:9092";
        private const string NewRequestsTopic = "requests";
        private const string UpdateRequestTopic = "update-request";

        public async Task ProduceNewRequestMessageAsync(string message)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = KafkaBootstrapServers
            };

            using (var producer = new ProducerBuilder<Null, string>(config).Build())
            {
                try
                {
                    var result = await producer.ProduceAsync(NewRequestsTopic, new Message<Null, string> { Value = message });
                    Console.WriteLine($"Message '{message}' sent to {result.TopicPartitionOffset}");
                }
                catch (ProduceException<Null, string> ex)
                {
                    Console.WriteLine($"Error producing message: {ex.Error.Reason}");
                }
            }
        }

        public async Task ProduceRequestUpdatedMessageAsync(string message)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = KafkaBootstrapServers
            };

            using (var producer = new ProducerBuilder<Null, string>(config).Build())
            {
                try
                {
                    var result = await producer.ProduceAsync(UpdateRequestTopic, new Message<Null, string> { Value = message });
                    Console.WriteLine($"Message '{message}' sent to {result.TopicPartitionOffset}");
                }
                catch (ProduceException<Null, string> ex)
                {
                    Console.WriteLine($"Error producing message: {ex.Error.Reason}");
                }
            }
        }
    }
}
