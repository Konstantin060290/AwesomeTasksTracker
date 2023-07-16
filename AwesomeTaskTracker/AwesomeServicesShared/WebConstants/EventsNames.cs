namespace TasksTrackerService.WebConstants;

public static class EventsNames
{
    public const string UserRegistered = "UserRegistered";

    public const string UserChanged = "UserChanged";

    public const string UserDeleted = "UserDeleted";

    public const string UserAuthenticateRequest = "UserAuthenticateRequest";
    
    public const string UserAuthenticated = "UserAuthenticated";
    
    public const string UserNotAuthenticated = "UserNotAuthenticated";
    
    public const string PriceWasRequested = "PriceWasRequested";
    
    public const string PriceWasCalculated = "PriceWasCalculated";
    
    public const string MoneyWrittenOff = "MoneyWrittenOff";
    
    public const string MoneyAccrued = "MoneyAccrued";
}