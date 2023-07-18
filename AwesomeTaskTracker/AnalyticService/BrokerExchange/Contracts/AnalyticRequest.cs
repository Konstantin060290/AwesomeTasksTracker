namespace AnalyticService.BrokerExchange.Contracts;

public class AnalyticRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string UserEmail { get; set; } = "";
}