using System.Text.Json;
using Confluent.Kafka;
using TasksTrackerService.BrokerCommon;
using TasksTrackerService.Context;
using TasksTrackerService.Models;
using TasksTrackerService.WebConstants;
using Task = System.Threading.Tasks.Task;

namespace TasksTrackerService.BrokerExchange;

public class UserConsumer : IUserConsumer
{
    private readonly ApplicationContext _context;

    public UserConsumer(ApplicationContext context)
    {
        _context = context;
        ConsumeUsers();
    }

    public void ConsumeUsers()
    {
        Task.Run(Listen);
    }

    private async void Listen()
    {
        using var builder = ConsumerCommon.BuildConsumer(KafkaConsumeGroupsNames.UserConsumer);

        builder.Subscribe(KafkaTopicNames.Account);

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
                    if (consumeResult.Message.Key == EventsNames.UserRegistered)
                    {
                        await AddNewUserToDb(consumeResult, cts);
                    }
                    if (consumeResult.Message.Key == EventsNames.UserChanged)
                    {
                        await ChangeUserInDb(consumeResult, cts);
                    }
                    if (consumeResult.Message.Key == EventsNames.UserDeleted)
                    {
                        await DeleteUserInDb(consumeResult, cts);
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

    private async Task AddNewUserToDb(ConsumeResult<string, string> consumeResult, CancellationTokenSource cts)
    {
        var user = JsonSerializer.Deserialize<User>(consumeResult.Message.Value);
        _context.Users.Add(user!);
        await _context.SaveChangesAsync(cts.Token);
    }
    private async Task ChangeUserInDb(ConsumeResult<string, string> consumeResult, CancellationTokenSource cts)
    {
        var changedUser = JsonSerializer.Deserialize<User>(consumeResult.Message.Value);
        if (changedUser is null)
        {
            return;
        }
        var existedUser = _context.Users.FirstOrDefault(u=>u.UserId == changedUser.UserId);
        if (existedUser is not null)
        {
            existedUser.UserName = changedUser.UserName;
            existedUser.UserRoleName = changedUser.UserRoleName;
            existedUser.Email = changedUser.Email;
        }
        await _context.SaveChangesAsync(cts.Token);
    }
    private async Task DeleteUserInDb(ConsumeResult<string, string> consumeResult, CancellationTokenSource cts)
    {
        var changedUser = JsonSerializer.Deserialize<User>(consumeResult.Message.Value);
        if (changedUser is null)
        {
            return;
        }
        var existedUser = _context.Users.FirstOrDefault(u=>u.UserId == changedUser.UserId);
        if (existedUser is not null)
        {
            _context.Users.Remove(existedUser);
        }
        await _context.SaveChangesAsync(cts.Token);
    }
}

public interface IUserConsumer
{
    void ConsumeUsers();
}