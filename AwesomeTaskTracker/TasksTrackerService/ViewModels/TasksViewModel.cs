using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TasksTrackerService.ViewModels;

public class TasksViewModel
{
    public List<TaskViewModel> Tasks { get; set; } = new();
    public string UserRole { get; set; } = "";
    public string UserEmail { get; set; } = "";
}

public class TaskViewModel
{
    [Display(Name = "Id")]
    public int TaskId { get; set; }
    
    [Display(Name = "Описание задачи")]
    public string TaskDescription { get; set; } = "";
    
    [Display(Name = "Ответственный попуг")]
    public string Responsible { get; set; } = "";
    
    [Display(Name = "Статус задачи")]
    public string Status { get; set; } = "";
    
    [Display(Name = "Все попуги")]
    public SelectList? AllUsers { get; set; }
    
    [Display(Name = "Стоимость задачи")]
    public string Price { get; set; } = "";
}