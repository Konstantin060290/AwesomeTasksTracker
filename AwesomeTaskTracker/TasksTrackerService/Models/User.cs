using System.ComponentModel.DataAnnotations;

namespace TasksTrackerService.Models;

public class User
{
    [Key]
    public int UserId { get; set; }
    public string Email { get; set; } = "";
    public string UserRoleName { get; set; } = "";
    public string UserName { get; set; } = "";

    public bool IsAuthenticated;
}