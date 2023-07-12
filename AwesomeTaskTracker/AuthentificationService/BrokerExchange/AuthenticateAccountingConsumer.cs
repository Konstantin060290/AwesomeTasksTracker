using System.Text;
using Confluent.Kafka;
using TasksTrackerService.BrokerCommon;
using TasksTrackerService.WebConstants;

namespace AuthentificationService.BrokerExchange;

public class AuthenticateAccountingConsumer : IAuthenticateAccountingConsumer
{
    public AuthenticateAccountingConsumer()
    {
        ConsumeAuthenticate();
    }

    public void ConsumeAuthenticate()
    {
        Task.Run(Listen);
    }

    private async void Listen()
    {
        using var builder = ConsumerCommon.BuildConsumer(KafkaConsumeGroupsNames.AuthenticateConsumer);

        builder.Subscribe(KafkaTopicNames.AccountingAuthRequests);

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

                    if (consumeResult.Message.Key == EventsNames.UserAuthenticateRequest)
                    {
                        await AuthenticateAccounting(consumeResult, cts);
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

    private static async Task AuthenticateAccounting(ConsumeResult<string, string> consumeResult,
        CancellationTokenSource cts)
    {
        if (Uri.TryCreate(new Uri("http://localhost:5154", UriKind.Absolute),
                $"AuthenticateBroker/AuthenticateUserInAccounting", out var uri))
        {
            var client = new HttpClient();

            var stringContent = new StringContent(consumeResult.Message.Value, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(uri, stringContent, cts.Token);

            response.EnsureSuccessStatusCode();
        }
        else
        {
            throw new InvalidOperationException("Failed to compose address for sending documents data");
        }
    }
}

public interface IAuthenticateAccountingConsumer
{
    void ConsumeAuthenticate();
}