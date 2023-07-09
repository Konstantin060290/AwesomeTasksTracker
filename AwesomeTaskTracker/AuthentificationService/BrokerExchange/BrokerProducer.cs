using System.Text.Json;
using AuthentificationService.BrokerExchange.Contracts;
using AuthentificationService.Models;
using Confluent.Kafka;
using TasksTrackerService.BrokerCommon;
using TasksTrackerService.WebConstants;

namespace AuthentificationService.BrokerExchange;

public class BrokerProducer
{
    public async Task SendUserToBroker(string userRole, User user, string eventName)
    {
        var userMessage = new UserMessage(user)
        {
            UserRoleName = userRole
        };

        var newUserMessage = JsonSerializer.Serialize(userMessage);
        
        var producerBuilder = ProducerCommon.BuildProducer();
        
        await producerBuilder.ProduceAsync(KafkaTopicNames.Account, new Message<string, string>
        {
            Key = eventName,
            Value = newUserMessage
        });

        producerBuilder.Flush(TimeSpan.FromSeconds(10));
    }

    public async Task SendAuthConfirm(string userEmail, string eventName)
    {
        var authConfirmMessage = new AuthConfirm
        {
            UserEmail = userEmail
        };

        var message = JsonSerializer.Serialize(authConfirmMessage);
        
        var producerBuilder = ProducerCommon.BuildProducer();
        
        await producerBuilder.ProduceAsync(KafkaTopicNames.TaskTrackerAuthAnswers, new Message<string, string>
        {
            Key = eventName,
            Value = message
        });

        producerBuilder.Flush(TimeSpan.FromSeconds(10));
    }
}