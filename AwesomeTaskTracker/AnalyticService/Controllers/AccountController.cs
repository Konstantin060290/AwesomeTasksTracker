using System.Runtime.Caching;
using AnalyticService.BrokerExchange;
using AnalyticService.Context;
using AnalyticService.Models;
using AnalyticService.ViewModels;
using Microsoft.AspNetCore.Mvc;
using TasksTrackerService.WebConstants;

namespace AnalyticService.Controllers;

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
        
        if (!AuthChecker.IsUserAuthenticated(_context, out var userRoleName, out var userEmail))
        {
            return Unauthorized("Попуг не аутентифицирован");
        }
        if (userRoleName != RoleNames.AdminRole)
        {
            return Unauthorized("У попуга нет прав на просмотр аналитики");
        }
        
        if (result is OkResult)
        {
            return RedirectToAction("Index", "Home");   
        }

        return result;
    }
}