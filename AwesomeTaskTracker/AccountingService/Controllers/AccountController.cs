using System.Runtime.Caching;
using AccountingService.BrokerExchange;
using AccountingService.Context;
using AccountingService.ViewModels;
using Microsoft.AspNetCore.Mvc;
using TasksTrackerService.WebConstants;

namespace AccountingService.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationContext _context;
    private readonly BrokerProducer _brokerProducer;

    public AccountController(ApplicationContext context)
    {
        _context = context;
        _brokerProducer = new BrokerProducer();
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
        await _brokerProducer.SendAuthenticateRequest(viewModel.Email, viewModel.BeakShape,
            EventsNames.UserAuthenticateRequest);

        var consumer = new AuthConsumer();
        var result = consumer.ConsumeAuthenticateConfirm(viewModel.Email);
        if (result is OkResult)
        {
            return RedirectToAction("Index", "Home");   
        }

        return result;
    }
}