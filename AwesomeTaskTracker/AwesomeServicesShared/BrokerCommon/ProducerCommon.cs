using Confluent.Kafka;

namespace TasksTrackerService.BrokerCommon;

public static class ProducerCommon
{
    public static IProducer<string, string> BuildProducer()
    {
        var config = new ProducerConfig
        {
            BootstrapServers = "localhost:19092"
        };

        var producerBuilder = new ProducerBuilder<string, string>(config).Build();
        return producerBuilder;
    }
}