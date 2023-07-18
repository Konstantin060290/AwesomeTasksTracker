using System.Runtime.InteropServices.JavaScript;

namespace AnalyticService.ViewModels;

public class AnalyticsViewModel
{
    public decimal TopManagersEarnsMoney { get; set; }

    public int PopugsQtyWithNegativeBalance { get; set; }
    
    public decimal MostExpensiveTaskForDay { get; set; }
    public decimal MostExpensiveTaskForPeriod { get; set; }

    public string? StartDate { get; set; } = "";
    public string? FinishDate { get; set; } = "";
}