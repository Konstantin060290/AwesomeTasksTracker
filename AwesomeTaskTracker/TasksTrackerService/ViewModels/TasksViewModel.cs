namespace TasksTrackerService.ViewModels;

public class TasksViewModel
{
    public List<TaskViewModel> Tasks { get; set; } = new();
    public string UserRole { get; set; } = "";
    public string UserEmail { get; set; } = "";
}

public class TaskViewModel
{
    public int TaskId { get; set; }
    public string TaskDescription { get; set; } = "";
    public string Responsible { get; set; } = "";
    public string Price { get; set; } = "";
}