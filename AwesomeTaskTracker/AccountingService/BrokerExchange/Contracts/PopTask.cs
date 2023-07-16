namespace AccountingService.BrokerExchange.Contracts;

public class PopTask
{
    public int Id { get; set; }
    public string TaskDescription { get; set; }
    public string Responsible { get; set; }
    public string Status { get; set; }
    public decimal PriceAssignedTask { get; set; }
    public decimal PriceCompletedTask { get; set; }
}