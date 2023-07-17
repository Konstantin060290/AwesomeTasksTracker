using System.Runtime.Caching;
using System.Text.Json;
using AnalyticService.BrokerExchange.Contracts;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using TasksTrackerService.BrokerCommon;
using TasksTrackerService.WebConstants;

namespace AnalyticService.BrokerExchange;

public class AuthConsumer : Controller
{
    public IActionResult  ConsumeAuthenticateConfirm(string email)
    {
        using var builder = ConsumerCommon.BuildConsumer(KafkaConsumeGroupsNames.AuthenticateConsumer);

        builder.Subscribe(KafkaTopicNames.AnalyticAuthAnswers);

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
                    var userMail = JsonSerializer.Deserialize<AuthConfirm>(consumeResult.Message.Value)!.UserEmail;
                    if (email == userMail)
                    {
                        if (consumeResult.Message.Key == EventsNames.UserAuthenticated)
                        {
                            ObjectCache cache = MemoryCache.Default;

                            var userAuth = new Dictionary<string, bool> { { userMail, true } };

                            var cacheItemPolicy = new CacheItemPolicy
                            {
                            };
     
                            cache.Set(EventsNames.UserAuthenticated, userAuth, cacheItemPolicy);
                            
                            break;
                        }

                        if (consumeResult.Message.Key == EventsNames.UserNotAuthenticated)
                        {
                            return NotFound($"Попуга {email} не удалось аутентифицировать...");
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

        return Ok();
    }
}