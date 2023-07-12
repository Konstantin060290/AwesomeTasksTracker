namespace TasksTrackerService.WebConstants;

public abstract class KafkaTopicNames
{
    public const string Account = "Account";
    
    public const string TaskTrackerAuthRequests = "TaskTrackerAuthRequests";
    
    public const string TaskTrackerAuthAnswers = "TaskTrackerAuthAnswers";
    
    public const string AccountingAuthRequests = "AccountingAuthRequests";
    
    public const string AccountingAuthAnswers = "AccountingAuthAnswers";
    
    public const string TaskTrackerPriceRequests = "TaskTrackerPriceRequests";
    
    public const string TaskTrackerPriceAnswers = "TaskTrackerPriceAnswers";
}