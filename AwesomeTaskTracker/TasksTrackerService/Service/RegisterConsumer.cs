using Confluent.Kafka;

namespace TasksTrackerService.Service;

public class RegisterConsumer : IRegisterConsumer
{
    public void ConsumeRegisterUser()
    {
        Task.Run(Listen);
    }

    private static void Listen()
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = "localhost:19092",
            GroupId = "testConsume"
        };

        using var builder = new ConsumerBuilder<string, string>(config).Build();
        builder.Subscribe("Account");

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

                    Console.WriteLine(
                        $"Consumed message '{consumeResult.Message.Value}' at: '{consumeResult.TopicPartitionOffset}'.");
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

public interface IRegisterConsumer
{
}