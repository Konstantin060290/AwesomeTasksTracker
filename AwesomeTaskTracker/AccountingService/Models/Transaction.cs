namespace AccountingService.Models;

public class Transaction
{
    public int TransactionId  { get; set; }
    public int BillId { get; set; }
    public decimal Accrued { get; set; }
    public decimal WrittenOff { get; set; }
    public string TaskDescription { get; set; } = "";
}