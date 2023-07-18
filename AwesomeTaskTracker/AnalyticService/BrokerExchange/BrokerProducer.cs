using System.Text.Json;
using AnalyticService.BrokerExchange.Contracts;
using Confluent.Kafka;
using TasksTrackerService.BrokerCommon;
using TasksTrackerService.WebConstants;

namespace AnalyticService.BrokerExchange;

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
        
        await producerBuilder.ProduceAsync(KafkaTopicNames.AnalyticAuthRequests, new Message<string, string>
        {
            Key = eventName,
            Value = newAuthMessage
        });

        producerBuilder.Flush(TimeSpan.FromSeconds(10));
    }
    
    public async Task SendAnalyticRequest(string userMail, DateTime startDate, DateTime endDate, string eventName)
    {
        var userAuthentification = new AnalyticRequest()
        {
            UserEmail = userMail,
            StartDate = startDate,
            EndDate = endDate
        };

        var newAuthMessage = JsonSerializer.Serialize(userAuthentification);

        var producerBuilder = ProducerCommon.BuildProducer();
        
        await producerBuilder.ProduceAsync(KafkaTopicNames.AnalyticRequests, new Message<string, string>
        {
            Key = eventName,
            Value = newAuthMessage
        });

        producerBuilder.Flush(TimeSpan.FromSeconds(10));
    }
}