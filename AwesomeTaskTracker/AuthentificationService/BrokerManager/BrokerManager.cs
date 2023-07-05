using System.Text.Json;
using AuthentificationService.BrokerManager.Contracts;
using AuthentificationService.Models;
using Confluent.Kafka;

namespace AuthentificationService.BrokerManager;

public class BrokerManager
{
    public async Task SendUserToBroker(string userRole, User user, string eventName)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = "localhost:19092"
        };

        var userMessage = new UserMessage(user)
        {
            UserRoleName = userRole
        };

        var newUserMessage = JsonSerializer.Serialize(userMessage);

        var producerBuilder = new ProducerBuilder<string, string>(config).Build();
        await producerBuilder.ProduceAsync("Account", new Message<string, string>
        {
            Key = eventName,
            Value = newUserMessage
        });

        producerBuilder.Flush(TimeSpan.FromSeconds(10));
    }
}