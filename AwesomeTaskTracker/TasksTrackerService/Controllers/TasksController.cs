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
        if (!AuthChecker.IsUserAuthenticated(_context, out var userRoleName, out var userEmail))
        {
            return Unauthorized("Попуг не аутентифицирован");
        }

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
        if (userRoleName is WebConstants.WebConstants.AdminRole or WebConstants.WebConstants.ManagerRole)
        {
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
        }
        else
        {
            if (tasks.Count > 0)
            {
                foreach (var task in tasks.Where(t=>t.Responsible == userEmail).ToList())
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
        }

        return View(viewModel);
    }
    
    [HttpGet]
    public IActionResult CreateTask(string email)
    {
        if (!AuthChecker.IsUserAuthenticated(_context, out var userRoleName, out var userEmail))
        {
            return Unauthorized("Попуг не аутентифицирован");
        }
        
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
            var mail = user.Email;
            
            viewModel.AllUsers = new SelectList(new List<SelectListItem>
            {
                new() { Text = mail, Value = mail },
            }, "Value", "Text");
        }

        viewModel.Status = "New";
        viewModel.Price = "100";
        
        return View(viewModel);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateTask(TaskViewModel viewModel)
    {
        if (!AuthChecker.IsUserAuthenticated(_context, out var userRoleName, out var userEmail))
        {
            return Unauthorized("Попуг не аутентифицирован");
        }
        
        var task = new PopTask
        {
            TaskDescription = viewModel.TaskDescription,
            Responsible = viewModel.Responsible,
            Price = Convert.ToDouble(viewModel.Price),
            Status = WebConstants.TasksStatuses.NewTaskStatus
        };
        await _context.PopTasks.AddAsync(task);
        await _context.SaveChangesAsync();
        
        return RedirectToAction("ManageTasks", new { email = userEmail });
    }
    
    [HttpPost]
    public async Task<IActionResult>AssignTasks()
    {
        if (!AuthChecker.IsUserAuthenticated(_context, out var userRoleName, out var userEmail))
        {
            return Unauthorized("Попуг не аутентифицирован");
        }
        if (userRoleName is WebConstants.WebConstants.AdminRole or WebConstants.WebConstants.ManagerRole)
        {
            var tasks = _context.PopTasks.ToList().Where(t=>t.Status!= WebConstants.TasksStatuses.FinishedTaskStatus);
            var users = _context.Users.ToArray();
        
            foreach (var task in tasks)
            {
                var random = new Random();
                var index = random.Next(0, users.Length);
                task.Responsible = users[index].Email;
            }
        
            await _context.SaveChangesAsync();

            return RedirectToAction("ManageTasks", new { email = userEmail });

        }

        return Unauthorized("Попуг не имеет прав");
    }
    
    [HttpPost]
    public async Task<IActionResult>FinishTask(int id)
    {
        if (!AuthChecker.IsUserAuthenticated(_context, out var userRoleName, out var userEmail))
        {
            return Unauthorized("Попуг не аутентифицирован");
        }
        if (userRoleName is WebConstants.WebConstants.AdminRole or WebConstants.WebConstants.ManagerRole)
        {
            var tasks = _context.PopTasks.ToList();

            var taskForFinish = tasks.FirstOrDefault(t => t.Id == id);

            taskForFinish!.Status = WebConstants.TasksStatuses.FinishedTaskStatus;

            await _context.SaveChangesAsync();

            return RedirectToAction("ManageTasks", new { email = userEmail });

        }

        return Unauthorized("Попуг не имеет прав");
    }

}