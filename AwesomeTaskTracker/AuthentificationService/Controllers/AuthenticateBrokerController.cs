using AuthentificationService.BrokerManager.Contracts;
using AuthentificationService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthentificationService.Controllers;

public class AuthenticateBrokerController : Controller
{
    private readonly SignInManager<User> _signInManager;
    private readonly BrokerManager.BrokerManager _brokerManager;

    public AuthenticateBrokerController(SignInManager<User> signInManager)
    {
        _signInManager = signInManager;
        _brokerManager = new BrokerManager.BrokerManager();
    }
    
    [HttpPost]
    [AllowAnonymous]
    public async Task AuthenticateUser([FromBody]UserAuthentification? userMessage, CancellationTokenSource cts)
    {
        if (userMessage is null)
        {
            return;
        }
        var result = await _signInManager.PasswordSignInAsync(userMessage.UserEmail,
            userMessage.UserPassword, false, false);
        if (result.Succeeded)
        {
            await _brokerManager.SendAuthConfirm(userMessage.UserEmail, WebConstants.EventsNames.UserAuthenticated);
        }
        else
        {
            await _brokerManager.SendAuthConfirm(userMessage.UserEmail, WebConstants.EventsNames.UserNotAuthenticated);
        }
    }
}