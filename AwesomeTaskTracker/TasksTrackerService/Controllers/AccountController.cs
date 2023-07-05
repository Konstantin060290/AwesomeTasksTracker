using AuthentificationService.WebConstants;
using Microsoft.AspNetCore.Mvc;
using TasksTrackerService.ViewModels;

namespace TasksTrackerService.Controllers;

public class AccountController : Controller
{
    private readonly BrokerManager.BrokerManager _brokerManager;

    public AccountController()
    {
        _brokerManager = new BrokerManager.BrokerManager();
    }

    public IActionResult Logout()
    {
        throw new NotImplementedException();
    }

    [HttpGet]
    public IActionResult Login()
    {
        var loginViewModel = new LoginViewModel();
        TempData["IsAuthenticate"] = false;
        return View(loginViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel viewModel)
    {
        await _brokerManager.SendAuthenticate(viewModel.Email, viewModel.BeakShape,
            EventsNames.UserAuthenticateRequest);
        return RedirectToAction("Index", "Home");
    }
}