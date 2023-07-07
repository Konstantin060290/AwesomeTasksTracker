using AuthentificationService.BrokerManager.Contracts;
using AuthentificationService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthentificationService.Controllers;

public class AuthenticateBrokerController : Controller
{
    private readonly SignInManager<User> _signInManager;

    public AuthenticateBrokerController(SignInManager<User> signInManager)
    {
        _signInManager = signInManager;
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
            //TODO Отправить в брокер результат
        }
    }
}