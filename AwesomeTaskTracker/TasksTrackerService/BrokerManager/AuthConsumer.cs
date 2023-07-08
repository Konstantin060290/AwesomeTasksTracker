using System.Runtime.Caching;
using System.Text.Json;
using AuthentificationService.WebConstants;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using TasksTrackerService.BrokerManager.Contracts;

namespace TasksTrackerService.BrokerManager;

public class AuthConsumer : Controller
{
    public IActionResult  ConsumeAuthenticate(string email)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = "localhost:19092",
            GroupId = "AuthConsumer"
        };

        using var builder = new ConsumerBuilder<string, string>(config).Build();
        builder.Subscribe(KafkaTopicNames.TaskTrackerAuthAnswers);

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

                            var cacheItemPolicy = new CacheItemPolicy
                            {
                            };
                            
                            cache.Set(EventsNames.UserAuthenticated, true, cacheItemPolicy);
                            
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