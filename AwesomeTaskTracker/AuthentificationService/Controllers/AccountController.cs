using AuthentificationService.Models;
using AuthentificationService.ViewModels;
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
    public IActionResult Register()
    {
        var roles = _context.Roles;
        var viewModel = new RegisterViewModel();

        foreach (var role in roles)
        {
            viewModel.Roles.Add(new SelectListItem(role.Name, role.Name));
        }

        viewModel.SelectedRole = new SelectListItem()
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
                // установка куки
                //await _signInManager.SignInAsync(user, false);

                await SetNewUserRights(model);

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
        await _context.UserRoles.AddAsync(new IdentityUserRole<int>() { RoleId = roleId, UserId = userId });
        await _context.SaveChangesAsync();
    }

    [HttpGet] 
    public IActionResult Login(string returnUrl = "") { 
        var model = new LoginViewModel { ReturnUrl = returnUrl }; 
        return View(model); 
    }
    
    [HttpPost]
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
    public async Task<IActionResult> Logout() { 
        await _signInManager.SignOutAsync(); 
        return RedirectToAction("Index", "Home");
    } 
}