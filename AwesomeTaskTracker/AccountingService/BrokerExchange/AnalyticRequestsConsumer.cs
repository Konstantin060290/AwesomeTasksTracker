using System.Globalization;
using System.Text.Json;
using AccountingService.BrokerExchange.Contracts;
using AccountingService.Context;
using AccountingService.Models;
using Confluent.Kafka;
using TasksTrackerService.BrokerCommon;
using TasksTrackerService.WebConstants;

namespace AccountingService.BrokerExchange;

public class AnalyticRequestsConsumer : IAnalyticRequestsConsumer
{
    private readonly ApplicationContext _context;
    private readonly BrokerProducer _brokerProducer;

    public AnalyticRequestsConsumer(ApplicationContext context)
    {
        _context = context;
        _brokerProducer = new BrokerProducer();
        ConsumeAnalyticRequests();
    }

    public void ConsumeAnalyticRequests()
    {
        Task.Run(Listen);
    }

    private async void Listen()
    {
        using var builder = ConsumerCommon.BuildConsumer(KafkaConsumeGroupsNames.AnalyticConsumer);

        builder.Subscribe(KafkaTopicNames.AnalyticRequests);

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
                    if (consumeResult.Message.Key == EventsNames.AnalyticWasRequested)
                    {
                        var analyticRequest = JsonSerializer.Deserialize<AnalyticRequest>(consumeResult.Message.Value);
                        
                        var analyticAnswer = new AnalyticAnswer();
                        
                        List<Transaction> transactions;
                        if (analyticRequest!.EndDate.Date > new DateTime(1, 1, 1))
                        {
                            transactions =
                                _context.Transactions.ToList()
                                    .Where(t => t.TransactionDate >= analyticRequest!.StartDate && t.TransactionDate <= analyticRequest.EndDate)
                                    .ToList();
                            if (transactions.Count > 0)
                            {
                                var theMostExpensive = transactions.Select(t => t.Accrued).Max();
                                analyticAnswer.MostExpensiveTaskForPeriod = theMostExpensive;
                            }

                        }
                        else
                        {
                            transactions =_context.Transactions.ToList()
                                .Where(t => t.TransactionDate.Date == analyticRequest!.StartDate.Date)
                                .ToList();
                            if (transactions.Count > 0)
                            {
                                var theMostExpensive = transactions.Select(t => t.Accrued).Max();
                                analyticAnswer.MostExpensiveTaskForDay = theMostExpensive;
                            }
                        }

                        analyticAnswer.StartDate = analyticRequest.StartDate.ToString(CultureInfo.InvariantCulture);
                        analyticAnswer.FinishDate = analyticRequest.EndDate.ToString(CultureInfo.InvariantCulture);
                        analyticAnswer.UserEmail = analyticRequest.UserEmail;

                        await _brokerProducer.SendAnalyticAnswer(analyticAnswer, EventsNames.AnalyticWasGiven);
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

public interface IAnalyticRequestsConsumer
{
    void ConsumeAnalyticRequests();
}