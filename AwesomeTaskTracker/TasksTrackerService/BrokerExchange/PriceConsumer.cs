using System.Text.Json;
using Confluent.Kafka;
using TasksTrackerService.BrokerCommon;
using TasksTrackerService.BrokerExchange.Contracts;
using TasksTrackerService.WebConstants;

namespace TasksTrackerService.BrokerExchange;

public class PriceConsumer
{
    public PriceAnswer? ConsumePrice(int taskId)
    {
        using var builder = ConsumerCommon.BuildConsumer(KafkaConsumeGroupsNames.PriceConsumer);

        builder.Subscribe(KafkaTopicNames.TaskTrackerPriceAnswers);

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
                    var priceAnswer = JsonSerializer.Deserialize<PriceAnswer>(consumeResult.Message.Value)!;
                    if (taskId == priceAnswer.TaskId)
                    {
                        if (consumeResult.Message.Key == EventsNames.PriceWasCalculated)
                        {
                            return priceAnswer;
                        }

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

        return null;
    }
}