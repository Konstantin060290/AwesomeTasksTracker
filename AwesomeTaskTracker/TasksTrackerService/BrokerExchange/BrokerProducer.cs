using System.Text.Json;
using Confluent.Kafka;
using TasksTrackerService.BrokerCommon;
using TasksTrackerService.BrokerExchange.Contracts;
using TasksTrackerService.WebConstants;

namespace TasksTrackerService.BrokerExchange;

public class BrokerProducer
{
    public async Task SendAuthenticateRequest(string userMail, string userPassword, string eventName)
    {
        var userAuthentification = new UserAuthentification
        {
            UserPassword = userPassword,
            UserEmail = userMail
        };

        var newAuthMessage = JsonSerializer.Serialize(userAuthentification);

        var producerBuilder = ProducerCommon.BuildProducer();
        
        await producerBuilder.ProduceAsync(KafkaTopicNames.TaskTrackerAuthRequests, new Message<string, string>
        {
            Key = eventName,
            Value = newAuthMessage
        });

        producerBuilder.Flush(TimeSpan.FromSeconds(10));
    }
}