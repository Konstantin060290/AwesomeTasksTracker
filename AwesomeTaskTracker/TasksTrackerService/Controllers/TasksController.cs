using Microsoft.AspNetCore.Mvc;
using TasksTrackerService.Context;
using TasksTrackerService.ViewModels;

namespace TasksTrackerService.Controllers;

public class TasksController : Controller
{
    private readonly ApplicationContext _context;

    public TasksController(ApplicationContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult ManageTasks(string email)
    {
        var dbUser = _context.Users.FirstOrDefault(u => u.Email == email);
        if (dbUser is null)
        {
            return NotFound($"Не найден попуг с email: {email}");
        }

        var task1 = new TaskViewModel
        {
            TaskId = 1,
            TaskDescription = "Сделай хрень",
            Price = "50",
            Responsible = "rfrsk@yandex.ru"
        };
        var viewModel = new TasksViewModel();
        viewModel.Tasks.Add(task1);
        viewModel.UserEmail = email;
        viewModel.UserRole = dbUser.UserRoleName;
        return View(viewModel);
    }
}