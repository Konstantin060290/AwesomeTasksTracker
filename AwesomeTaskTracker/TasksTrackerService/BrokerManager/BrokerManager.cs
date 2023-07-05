using System.Text.Json;
using Confluent.Kafka;

namespace TasksTrackerService.BrokerManager;

public class BrokerManager
{
    public async Task SendAuthenticate(string userMail, string userPassword, string eventName)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = "localhost:19092"
        };

        var userAuthentification = new UserAuthentification
        {
            UserPassword = userPassword,
            UserEmail = userMail
        };
        
        var newAuthMessage = JsonSerializer.Serialize(userAuthentification);

        var producerBuilder = new ProducerBuilder<string, string>(config).Build();
        await producerBuilder.ProduceAsync("Account", new Message<string, string>
        {
            Key = eventName,
            Value = newAuthMessage
        });

        producerBuilder.Flush(TimeSpan.FromSeconds(10));
    }
}