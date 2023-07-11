namespace AccountingService.BrokerExchange.Contracts;

public class PriceAnswer
{
    public int TaskId { get; set; }
    public double PriceAssignTask { get; set; }
    
    public double PriceCompleteTask { get; set; }
}