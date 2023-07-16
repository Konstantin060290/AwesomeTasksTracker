using System.ComponentModel.DataAnnotations;

namespace AccountingService.ViewModels;

public class BillViewModel
{
    [Display(Name = "Id")] public int BillId { get; set; }
    [Display(Name = "Email")] public string UserEmail { get; set; } = "";
    [Display(Name = "Баланс счета")] public decimal Balance { get; set; }
    [Display(Name = "Статус счета")] public string Status { get; set; } = "";
    
    public List<TransactionViewModel> TransactionViewModels = new();
}

public class TransactionViewModel
{
    public int TransactionId  { get; set; }
    public int BillId { get; set; }
    public decimal Accrued { get; set; }
    public decimal WrittenOff { get; set; }
    public string TaskDescription { get; set; } = "";
    
    public DateTime TransactionDate { get; set; }
}