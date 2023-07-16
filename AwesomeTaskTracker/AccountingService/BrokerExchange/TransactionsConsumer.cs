using System.Text.Json;
using AccountingService.BrokerExchange.Contracts;
using AccountingService.Context;
using AccountingService.Models;
using Confluent.Kafka;
using TasksTrackerService.BrokerCommon;
using TasksTrackerService.WebConstants;

namespace AccountingService.BrokerExchange;

public class TransactionsConsumer : ITransactionsConsumer
{
    private readonly ApplicationContext _context;

    public TransactionsConsumer(ApplicationContext context)
    {
        _context = context;
        ConsumeTransactions();
    }

    public void ConsumeTransactions()
    {
        Task.Run(Listen);
    }

    private async void Listen()
    {
        using var builder = ConsumerCommon.BuildConsumer(KafkaConsumeGroupsNames.TransactionsConsumer);

        builder.Subscribe(KafkaTopicNames.TaskTransactions);

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
                    if (consumeResult.Message.Key == EventsNames.MoneyWrittenOff)
                    {
                        await MoneyWrittenOff(consumeResult, cts);
                    }
                    if (consumeResult.Message.Key == EventsNames.MoneyAccrued)
                    {
                        await MoneyAccrued(consumeResult, cts);
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

    private async Task MoneyWrittenOff(ConsumeResult<string, string> consumeResult, CancellationTokenSource cts)
    {
        var task = JsonSerializer.Deserialize<PopTask>(consumeResult.Message.Value);
        var user = _context.Users.FirstOrDefault(u => u.Email == task!.Responsible);
        if (user is not null)
        {
            var bill = _context.Bills.FirstOrDefault(b => b.UserId == user.UserId);
            var transaction = new Transaction
            {
                BillId = bill!.BillId,
                Accrued = 0,
                WrittenOff = task!.PriceAssignedTask,
                TaskDescription = task.TaskDescription,
                TransactionDate = DateTime.UtcNow
            };
            _context.Transactions.Add(transaction);
        }
        await _context.SaveChangesAsync(cts.Token);
        
    }
    
    private async Task MoneyAccrued(ConsumeResult<string, string> consumeResult, CancellationTokenSource cts)
    {
        var task = JsonSerializer.Deserialize<PopTask>(consumeResult.Message.Value);
        var user = _context.Users.FirstOrDefault(u => u.Email == task!.Responsible);
        if (user is not null)
        {
            var bill = _context.Bills.FirstOrDefault(b => b.UserId == user.UserId);
            var transaction = new Transaction
            {
                BillId = bill!.BillId,
                Accrued = task!.PriceCompletedTask,
                WrittenOff = 0,
                TaskDescription = task.TaskDescription,
                TransactionDate = DateTime.UtcNow
            };
            _context.Transactions.Add(transaction);
        }
        await _context.SaveChangesAsync(cts.Token);
        
    }
}

public interface ITransactionsConsumer
{
    void ConsumeTransactions();
}