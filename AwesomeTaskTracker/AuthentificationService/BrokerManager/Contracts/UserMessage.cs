using AuthentificationService.Models;

namespace AuthentificationService.BrokerManager.Contracts;

public class UserMessage
{
    public UserMessage(User user)
    {
        UserName = user.UserName ?? "";
        UserId = user.Id;
        Email = user.Email ?? "";
    }
    public string UserName { get; set; }
    
    public int UserId { get; set; }
    
    public string Email { get; set; }

    public string UserRoleName { get; set; } = "";
}