using Confluent.Kafka;

namespace Fetch.Api
{
    public interface IKafkaProducer
    {
        Task ProduceMessageAsync(string message);
    }

    public class KafkaProducer : IKafkaProducer
    {
        private const string KafkaBootstrapServers = "kafka:9092";
        private const string Topic = "requests";

        public async Task ProduceMessageAsync(string message)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = KafkaBootstrapServers
            };

            using (var producer = new ProducerBuilder<Null, string>(config).Build())
            {
                try
                {
                    var result = await producer.ProduceAsync(Topic, new Message<Null, string> { Value = message });
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
