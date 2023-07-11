using System.ComponentModel.DataAnnotations;

namespace AccountingService.Models;

public class User
{
    [Key]
    public int UserId { get; set; }
    public string Email { get; set; } = "";
    public string UserRoleName { get; set; } = "";
    public string UserName { get; set; } = "";
}