using System.Text.Json;
using AnalyticService.BrokerExchange.Contracts;
using Confluent.Kafka;
using TasksTrackerService.BrokerCommon;
using TasksTrackerService.WebConstants;

namespace AnalyticService.BrokerExchange;

public class AnalyticConsumer
{
    public AnalyticAnswer ConsumeAnalyticAnswer(string email)
    {
        using var builder = ConsumerCommon.BuildConsumer(KafkaConsumeGroupsNames.AnalyticConsumer);

        builder.Subscribe(KafkaTopicNames.AnalyticAnswers);

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true; // prevent the process from terminating.
            cts.Cancel();
        };
        try
        {
            while (true)
            {
                try
                {
                    var consumeResult = builder.Consume(cts.Token);
                    var userMail = JsonSerializer.Deserialize<AnalyticAnswer>(consumeResult.Message.Value)!.UserEmail;
                    if (email == userMail)
                    {
                        return JsonSerializer.Deserialize<AnalyticAnswer>(consumeResult.Message.Value)!;
                    }
                }
                catch (ConsumeException e)
                {
                    Console.WriteLine($"Error occurred: {e.Error.Reason}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Ensure the consumer leaves the group cleanly and final offsets are committed.
            builder.Close();
        }

        return new AnalyticAnswer();
    }
}