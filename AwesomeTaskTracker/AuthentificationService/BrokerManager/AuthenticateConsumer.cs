using System.Text;
using AuthentificationService.Models;
using AuthentificationService.WebConstants;
using Confluent.Kafka;
using Microsoft.AspNetCore.Identity;

namespace AuthentificationService.BrokerManager;

public class AuthenticateConsumer : IAuthenticateConsumer
{
    private readonly ApplicationContext _context;
    private readonly SignInManager<User> _signInManager;
    private readonly IHttpContextAccessor _contextAccessor;

    public AuthenticateConsumer(ApplicationContext context, SignInManager<User> signInManager, IHttpContextAccessor contextAccessor)
    {
        _context = context;
        _signInManager = signInManager;
        _contextAccessor = contextAccessor;
        ConsumeAuthenticate();
    }

    public void ConsumeAuthenticate()
    {
        Task.Run(Listen);
    }

    private async void Listen()
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = "localhost:19092",
            GroupId = "AuthenticateConsumer"
        };

        using var builder = new ConsumerBuilder<string, string>(config).Build();
        builder.Subscribe(KafkaTopicNames.TaskTrackerAuthRequests);

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
                        if (Uri.TryCreate(new Uri("http://localhost:5154", UriKind.Absolute), $"AuthenticateBroker/AuthenticateUser", out var uri))
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

public interface IAuthenticateConsumer
{
    void ConsumeAuthenticate();
}