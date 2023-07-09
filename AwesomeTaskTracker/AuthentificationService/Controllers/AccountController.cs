using AuthentificationService.Models;
using AuthentificationService.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TasksTrackerService.WebConstants;

namespace AuthentificationService.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ApplicationContext _context;
    private readonly BrokerExchange.BrokerProducer _brokerProducer;

    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager,
        ApplicationContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _brokerProducer = new BrokerExchange.BrokerProducer();
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

                await _brokerProducer.SendUserToBroker(model.SelectedRole.Value, user, EventsNames.UserRegistered);

                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
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
    [Authorize(Roles = WebConstants.AdminRole)]
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
    [Authorize(Roles = WebConstants.AdminRole)]
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
        var roleName = _context.Roles.FirstOrDefault(r => r.Id == userRole!.RoleId)!.Name;

        await _brokerProducer.SendUserToBroker(roleName, user!, EventsNames.UserDeleted);
        
        return RedirectToAction("ManageUsers", "Account");
    }
}