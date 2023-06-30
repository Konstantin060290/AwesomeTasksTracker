using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AuthentificationService.Models;
using AuthentificationService.ViewModels;
using AuthentificationService.WebConstants;
using Confluent.Kafka;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AuthentificationService.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ApplicationContext _context;

    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager,
        ApplicationContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        var roles = _context.Roles;
        var viewModel = new RegisterViewModel();

        foreach (var role in roles)
        {
            viewModel.Roles.Add(new SelectListItem(role.Name, role.Name));
        }

        viewModel.SelectedRole = new SelectListItem
        {
            Value = "--Выберите роль попуга--"
        };

        return View(viewModel);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new User { Email = model.Email, UserName = model.Email };
            // добавляем пользователя

            try
            {
                var lastUseId = _context.Users.ToList().Select(u => u.Id).Max();
                user.Id = lastUseId + 1;
            }
            catch
            {
                user.Id = 1;
            }

            var result = await _userManager.CreateAsync(user, model.BeakShape);
            if (result.Succeeded)
            {
                await SetNewUserRights(model);

                await SendUserToBroker(model, user);

                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    private static async Task SendUserToBroker(RegisterViewModel model, User user)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = "localhost:19092"
        };

        var userMessage = new UserMessage(user)
        {
            UserRoleName = model.SelectedRole.Value
        };

        var newUserMessage = JsonSerializer.Serialize(userMessage);

        var producerBuilder = new ProducerBuilder<string, string>(config).Build();
        await producerBuilder.ProduceAsync("Account", new Message<string, string>
        {
            Key = EventsNames.UserRegistered,
            Value = newUserMessage
        });

        producerBuilder.Flush(TimeSpan.FromSeconds(10));
    }

    private async Task SetNewUserRights(RegisterViewModel model)
    {
        var userId = _context.Users.First(u => u.UserName == model.Email).Id;
        var roleId = _context.Roles.First(r => r.Name == model.SelectedRole.Value).Id;
        await _context.UserRoles.AddAsync(new IdentityUserRole<int> { RoleId = roleId, UserId = userId });
        await _context.SaveChangesAsync();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string returnUrl = "")
    {
        var model = new LoginViewModel { ReturnUrl = returnUrl };
        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email!,
                model.BeakShape!, false, false);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
        }

        ModelState.AddModelError("", "Invalid login attempt");
        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [Authorize(Roles = WebConstants.WebConstants.AdminRole)]
    public IActionResult ManageUsers()
    {
        var users = _context.Users.ToList();
        var userRoles = _context.UserRoles.ToList();
        var roles = _context.Roles.ToList();

        var assignRoleViewModel = new ManageUsersViewModel();

        foreach (var user in users)
        {
            var userRole = userRoles.FirstOrDefault(r => r.UserId == user.Id);
            var roleName = roles.FirstOrDefault(r => r.Id == userRole!.RoleId)!.Name;

            var userViewModel = new UserViewModel()
            {
                UserId = user.Id,
                UserName = user.UserName!,
                UserEmail = user.Email!,
                CurrentRoleName = roleName!,
                SelectedUserRole = roleName
            };

            assignRoleViewModel.Users.Add(userViewModel);
        }

        foreach (var role in roles)
        {
            assignRoleViewModel.Roles.Add(new SelectListItem(role.Name, role.Name));
        }

        return View(assignRoleViewModel);
    }

    [HttpPost]
    [Authorize(Roles = WebConstants.WebConstants.AdminRole)]
    public async Task<IActionResult> RemoveUser(int id)
    {
        var userRole = _context.UserRoles.ToList().FirstOrDefault(ur => ur.UserId == id);
        if (userRole is null)
        {
            RedirectToAction("ManageUsers");
        }

        _context.UserRoles.Remove(userRole!);
        var user = _context.Users.ToList().FirstOrDefault(u => u.Id == id);
        if (user is null)
        {
            RedirectToAction("ManageUsers");
        }

        _context.Users.Remove(user!);
        await _context.SaveChangesAsync();
        
        return RedirectToAction("ManageUsers", "Account");
    }
    
    [HttpGet]
    [Authorize(Roles = WebConstants.WebConstants.AdminRole)]
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
    [Authorize(Roles = WebConstants.WebConstants.AdminRole)]
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
        }
        else
        {
            return NotFound("Указанная роль не найдена");
        }
        
        return RedirectToAction("ManageUsers", "Account");
    }

    [HttpGet]
    [Authorize(Roles = WebConstants.WebConstants.AdminRole)]
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
    [Authorize(Roles = WebConstants.WebConstants.AdminRole)]
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

        return RedirectToAction("EditRoles", "Account");
    }


    [HttpPost]
    [Authorize(Roles = WebConstants.WebConstants.AdminRole)]
    public async Task<IActionResult> EditRole(RoleViewModel model, int id)
    {
        var role = _context.Roles.ToList().FirstOrDefault(r => r.Id == id);
        role!.Name = model.RoleName;
        await _context.SaveChangesAsync();
        return RedirectToAction("EditRoles", "Account");
    }

    [HttpPost]
    [Authorize(Roles = WebConstants.WebConstants.AdminRole)]
    public async Task<IActionResult> DeleteRole(int id)
    {
        var role = _context.Roles.ToList().FirstOrDefault(r => r.Id == id);
        _context.Roles.Remove(role!);
        await _context.SaveChangesAsync();
        return RedirectToAction("EditRoles", "Account");
    }
}