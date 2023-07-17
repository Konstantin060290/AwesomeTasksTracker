using System.ComponentModel.DataAnnotations;

namespace AnalyticService.Models;

public class User
{
    [Key]
    public int UserId { get; set; }
    public string Email { get; set; } = "";
    public string UserRoleName { get; set; } = "";
    public string UserName { get; set; } = "";
}