namespace AccountingService.ViewModels;

public class BillsViewModel
{
    public List<BillViewModel> Bills { get; set; } = new();
    public string UserRole { get; set; } = "";
    public string UserEmail { get; set; } = "";
}