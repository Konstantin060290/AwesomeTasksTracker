using System.Text.Json;
using AccountingService.BrokerExchange.Contracts;
using Confluent.Kafka;
using TasksTrackerService.BrokerCommon;
using TasksTrackerService.WebConstants;

namespace AccountingService.BrokerExchange;

public class PriceRequestsRequestsConsumer : IPriceRequestsConsumer
{
    private readonly BrokerProducer _brokerProducer;

    public PriceRequestsRequestsConsumer()
    {
        _brokerProducer = new BrokerProducer();
        ConsumePricesRequests();
    }

    public void ConsumePricesRequests()
    {
        Task.Run(Listen);
    }

    private async void Listen()
    {
        using var builder = ConsumerCommon.BuildConsumer(KafkaConsumeGroupsNames.PriceConsumer);

        builder.Subscribe(KafkaTopicNames.TaskTrackerPriceRequests);

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
                    if (consumeResult.Message.Key == EventsNames.PriceWasRequested)
                    {
                        var priceRequest = JsonSerializer.Deserialize<PriceRequest>(consumeResult.Message.Value);
                        var random = new Random();
                        var priceAssign = random.Next(-20, -10);
                        var priceCompleteTask = random.Next(20, 40);
                        await _brokerProducer.SendTaskPriceAnswer(priceRequest!.TaskId, priceAssign, priceCompleteTask, EventsNames.PriceWasCalculated);
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
    }
}

public interface IPriceRequestsConsumer
{
    void ConsumePricesRequests();
}