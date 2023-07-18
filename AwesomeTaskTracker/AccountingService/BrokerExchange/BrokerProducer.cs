using System.Text.Json;
using AccountingService.BrokerExchange.Contracts;
using Confluent.Kafka;
using TasksTrackerService.BrokerCommon;
using TasksTrackerService.WebConstants;

namespace AccountingService.BrokerExchange;

public class BrokerProducer
{
    public async Task SendTaskPriceAnswer(int taskId, double priceAssign, double priceCompleteTask, string eventName)
    {
        var priceRequest = new PriceAnswer()
        {
            TaskId = taskId,
            PriceAssignTask = priceAssign,
            PriceCompleteTask = priceCompleteTask
        };

        var message = JsonSerializer.Serialize(priceRequest);

        var producerBuilder = ProducerCommon.BuildProducer();
        
        await producerBuilder.ProduceAsync(KafkaTopicNames.TaskTrackerPriceAnswers, new Message<string, string>
        {
            Key = eventName,
            Value = message
        });

        producerBuilder.Flush(TimeSpan.FromSeconds(10));
    }
    
    public async Task SendAuthenticateRequest(string userMail, string userPassword, string eventName)
    {
        var userAuthentification = new UserAuthentification
        {
            UserPassword = userPassword,
            UserEmail = userMail
        };

        var newAuthMessage = JsonSerializer.Serialize(userAuthentification);

        var producerBuilder = ProducerCommon.BuildProducer();
        
        await producerBuilder.ProduceAsync(KafkaTopicNames.AccountingAuthRequests, new Message<string, string>
        {
            Key = eventName,
            Value = newAuthMessage
        });

        producerBuilder.Flush(TimeSpan.FromSeconds(10));
    }
    
    public async Task SendAnalyticAnswer(AnalyticAnswer analyticAnswer, string eventName)
    {
        var message = JsonSerializer.Serialize(analyticAnswer);

        var producerBuilder = ProducerCommon.BuildProducer();
        
        await producerBuilder.ProduceAsync(KafkaTopicNames.AnalyticAnswers, new Message<string, string>
        {
            Key = eventName,
            Value = message
        });

        producerBuilder.Flush(TimeSpan.FromSeconds(10));
    }
}