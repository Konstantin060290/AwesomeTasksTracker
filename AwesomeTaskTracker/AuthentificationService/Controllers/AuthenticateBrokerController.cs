using AuthentificationService.BrokerExchange.Contracts;
using AuthentificationService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TasksTrackerService.WebConstants;

namespace AuthentificationService.Controllers;

public class AuthenticateBrokerController : Controller
{
    private readonly SignInManager<User> _signInManager;
    private readonly BrokerExchange.BrokerProducer _brokerProducer;

    public AuthenticateBrokerController(SignInManager<User> signInManager)
    {
        _signInManager = signInManager;
        _brokerProducer = new BrokerExchange.BrokerProducer();
    }
    
    [HttpPost]
    [AllowAnonymous]
    public async Task AuthenticateUserInTaskTracker([FromBody]UserAuthentification? userMessage, CancellationTokenSource cts)
    {
        if (userMessage is null)
        {
            return;
        }
        var result = await _signInManager.PasswordSignInAsync(userMessage.UserEmail,
            userMessage.UserPassword, false, false);
        if (result.Succeeded)
        {
            await _brokerProducer.SendAuthTaskTrackerConfirm(userMessage.UserEmail, EventsNames.UserAuthenticated);
        }
        else
        {
            await _brokerProducer.SendAuthTaskTrackerConfirm(userMessage.UserEmail, EventsNames.UserNotAuthenticated);
        }
    }
    
    [HttpPost]
    [AllowAnonymous]
    public async Task AuthenticateUserInAccounting([FromBody]UserAuthentification? userMessage, CancellationTokenSource cts)
    {
        if (userMessage is null)
        {
            return;
        }
        var result = await _signInManager.PasswordSignInAsync(userMessage.UserEmail,
            userMessage.UserPassword, false, false);
        if (result.Succeeded)
        {
            await _brokerProducer.SendAuthAccountingConfirm(userMessage.UserEmail, EventsNames.UserAuthenticated);
        }
        else
        {
            await _brokerProducer.SendAuthAccountingConfirm(userMessage.UserEmail, EventsNames.UserNotAuthenticated);
        }
    }
    
    [HttpPost]
    [AllowAnonymous]
    public async Task AuthenticateUserInAnalytic([FromBody]UserAuthentification? userMessage, CancellationTokenSource cts)
    {
        if (userMessage is null)
        {
            return;
        }
        var result = await _signInManager.PasswordSignInAsync(userMessage.UserEmail,
            userMessage.UserPassword, false, false);
        if (result.Succeeded)
        {
            await _brokerProducer.SendAuthAnalyticConfirm(userMessage.UserEmail, EventsNames.UserAuthenticated);
        }
        else
        {
            await _brokerProducer.SendAuthAnalyticConfirm(userMessage.UserEmail, EventsNames.UserNotAuthenticated);
        }
    }
}