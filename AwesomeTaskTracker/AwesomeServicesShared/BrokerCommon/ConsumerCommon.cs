using Confluent.Kafka;

namespace TasksTrackerService.BrokerCommon;

public static class ConsumerCommon
{
    public static IConsumer<string, string> BuildConsumer(string consumeGroupName)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = "localhost:19092",
            GroupId = consumeGroupName
        };

        var builder = new ConsumerBuilder<string, string>(config).Build();
        
        return builder;
    }
}