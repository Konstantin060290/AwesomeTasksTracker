using System.Runtime.Caching;
using AuthentificationService.WebConstants;
using Microsoft.AspNetCore.Mvc;
using TasksTrackerService.BrokerManager;
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
        MemoryCache.Default.Remove(EventsNames.UserAuthenticated);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Login()
    {
        var loginViewModel = new LoginViewModel();
        return View(loginViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel viewModel)
    {
        await _brokerManager.SendAuthenticate(viewModel.Email, viewModel.BeakShape,
            EventsNames.UserAuthenticateRequest);

        var consumer = new AuthConsumer();
        var result = consumer.ConsumeAuthenticate(viewModel.Email);
        if (result is OkResult)
        {
            return RedirectToAction("Index", "Home");   
        }

        return result;
    }
}