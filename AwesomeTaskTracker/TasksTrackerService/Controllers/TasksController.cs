using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TasksTrackerService.Context;
using TasksTrackerService.Models;
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
    public IActionResult ManageTasks(string? email)
    {
        var dbUser = _context.Users.FirstOrDefault(u => u.Email == email);
        if (dbUser is null)
        {
            if (email is not null)
            {
                return NotFound($"Не найден попуг с email: {email}"); 
            }
        }
        if (email is null)
        {
            dbUser = new User();
        }
        var viewModel = new TasksViewModel
        {
            UserEmail = email??"",
            UserRole = dbUser!.UserRoleName
        };

        var tasks = _context.PopTasks.Select(t => t).ToList();
        if (tasks.Count > 0)
        {
            foreach (var task in tasks)
            {
                var taskViewModel = new TaskViewModel
                {
                    TaskId = task.Id,
                    TaskDescription = task.TaskDescription,
                    Price = task.Price.ToString(CultureInfo.InvariantCulture),
                    Responsible = task.Responsible,
                    Status = task.Status
                };
                viewModel.Tasks.Add(taskViewModel);
            }
        }

        return View(viewModel);
    }
    
    [HttpGet]
    public IActionResult CreateTask(string email)
    {
        var allId = _context.PopTasks.Select(t => t.Id).ToList();
        var maxId = 0;
        
        if (allId.Count > 0)
        {
            maxId = allId.Max();  
        }

        var viewModel = new TaskViewModel
        {
            TaskId = maxId + 1
        };
        
        var allUsers = _context.Users.Select(u => u).ToList();
        foreach (var user in allUsers)
        {
            var userEmail = user.Email;
            
            viewModel.AllUsers = new SelectList(new List<SelectListItem>
            {
                new() { Text = userEmail, Value = userEmail },
            }, "Value", "Text");
        }

        viewModel.Status = "New";
        viewModel.Price = "100";
        
        return View(viewModel);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateTask(TaskViewModel viewModel)
    {
        var task = new PopTask
        {
            TaskDescription = viewModel.TaskDescription,
            Responsible = viewModel.Responsible,
            Price = Convert.ToDouble(viewModel.Price),
            Status = "New"
        };
        await _context.PopTasks.AddAsync(task);
        await _context.SaveChangesAsync();
        
        return RedirectToAction("ManageTasks", "Tasks");
    }
   
}