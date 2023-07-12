using AuthentificationService.Models;
using AuthentificationService.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TasksTrackerService.WebConstants;

namespace AuthentificationService.Controllers;

public class RoleController: Controller
{
    private readonly ApplicationContext _context;
    private readonly BrokerExchange.BrokerProducer _brokerProducer;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;

    public RoleController(UserManager<User> userManager, SignInManager<User> signInManager,
        ApplicationContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _brokerProducer = new BrokerExchange.BrokerProducer();
    }
    
    [HttpGet]
    [Authorize(Roles = RoleNames.AdminRole)]
    public IActionResult ChangeRole(int id)
    {
        var user = _context.Users.ToList().FirstOrDefault(u=>u.Id == id);
        var userRole = _context.UserRoles.ToList().FirstOrDefault(ur=>ur.UserId == id);
        var role = _context.Roles.ToList().FirstOrDefault(r=>r.Id == userRole!.RoleId);

        var userViewModel = new UserViewModel
        {
            UserName = user!.UserName!,
            UserId = user!.Id!,
            UserEmail = user!.Email!,
            CurrentRoleName = role!.Name!
        };
        foreach (var dbRole in _context.Roles.ToList())
        {
            var roleItem = new SelectListItem(dbRole.Name, dbRole.Name);
            userViewModel.Roles.Add(roleItem);
        }

        return View(userViewModel);
    }
    
    [HttpPost]
    [Authorize(Roles = RoleNames.AdminRole)]
    public async Task<IActionResult> ChangeRolePost(UserViewModel userViewModel)
    {
        var userCurrentRole = _context.UserRoles.ToList().FirstOrDefault(ur=>ur.UserId == userViewModel.UserId);
        var newDbRole = _context.Roles.ToList().FirstOrDefault(r=>r.Name == userViewModel.SelectedUserRole);
        if (newDbRole is not null)
        {
            var userId = userCurrentRole!.UserId;
            _context.UserRoles.Remove(userCurrentRole);
            await _context.AddAsync(new IdentityUserRole<int>
            {
                UserId = userId,
                RoleId = newDbRole.Id
            });
            await _context.SaveChangesAsync();

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user is not null)
            {
                await _brokerProducer.SendUserToBroker(userViewModel.SelectedUserRole, user, EventsNames.UserChanged);
            }
            else
            {
                return NotFound($"Не найден пользователь с id:{userId}");  
            }
        }
        else
        {
            return NotFound("Указанная роль не найдена");
        }
        
        return RedirectToAction("ManageUsers", "Account");
    }

    [HttpGet]
    [Authorize(Roles = RoleNames.AdminRole)]
    public IActionResult EditRoles()
    {
        var roles = _context.Roles.ToList();

        var roleViewModel = new RoleViewModel();

        foreach (var role in roles)
        {
            var viewModel = new RoleViewModel
            {
                RoleName = role.Name!,
                RoleId = role.Id
            };

            roleViewModel.ViewModels?.Add(viewModel);
        }

        return View(roleViewModel);
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.AdminRole)]
    public async Task<IActionResult> AddNewRole(RoleViewModel roleViewModel)
    {
        var maxId = _context.Roles.ToList().Select(r => r.Id).Max();
        var role = new Role
        {
            Id = maxId + 1,
            Name = roleViewModel.RoleName,
            NormalizedName = roleViewModel.RoleName,
            ConcurrencyStamp = roleViewModel.RoleName
        };
        await _context.Roles.AddAsync(role);
        await _context.SaveChangesAsync();

        return RedirectToAction("EditRoles", "Role");
    }


    [HttpPost]
    [Authorize(Roles = RoleNames.AdminRole)]
    public async Task<IActionResult> EditRole(RoleViewModel model, int id)
    {
        var role = _context.Roles.ToList().FirstOrDefault(r => r.Id == id);
        role!.Name = model.RoleName;
        await _context.SaveChangesAsync();
        return RedirectToAction("EditRoles", "Role");
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.AdminRole)]
    public async Task<IActionResult> DeleteRole(int id)
    {
        var role = _context.Roles.ToList().FirstOrDefault(r => r.Id == id);
        _context.Roles.Remove(role!);
        await _context.SaveChangesAsync();
        return RedirectToAction("EditRoles", "Role");
    }
}