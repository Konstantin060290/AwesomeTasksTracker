using System;
using System.Text.Json;
using System.Threading.Tasks;
using AuthentificationService.Models;
using AuthentificationService.ViewModels;
using AuthentificationService.WebConstants;
using Confluent.Kafka;

namespace AuthentificationService.BrokerManager;

public class BrokerManager
{
    public async Task SendNewUserToBroker(RegisterViewModel model, User user)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = "localhost:19092"
        };

        var userMessage = new UserMessage(user)
        {
            UserRoleName = model.SelectedRole.Value
        };

        var newUserMessage = JsonSerializer.Serialize(userMessage);

        var producerBuilder = new ProducerBuilder<string, string>(config).Build();
        await producerBuilder.ProduceAsync("Account", new Message<string, string>
        {
            Key = EventsNames.UserRegistered,
            Value = newUserMessage
        });

        producerBuilder.Flush(TimeSpan.FromSeconds(10));
    }
}