using System.ComponentModel.DataAnnotations;

namespace TasksTrackerService.Models;

public class PopTask
{
    [Key]
    public int Id { get; set; }
    public string TaskDescription { get; set; }
    public string Responsible { get; set; }
    public string Status { get; set; }
    public double PriceAssignedTask { get; set; }
    public double PriceCompletedTask { get; set; }
}