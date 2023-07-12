using System.ComponentModel.DataAnnotations;

namespace AccountingService.Models;

public class Bill
{
    [Key]
    public int BillId { get; set; }
    public int UserId { get; set; }
    public decimal Balance { get; set; }

    public string Status { get; set; } = "";
}