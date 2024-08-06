using System.Security.Claims;
using Domain.Identity;
using Domain.Rules;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

[AllowAnonymous]
[Controller]
public class GoogleAuthenticationController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public GoogleAuthenticationController(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet("/signin-with-google")]
    public IActionResult SignIn()
    {
        return Challenge(new AuthenticationProperties
        {
            RedirectUri = Url.Action("HandleGoogleResponse"),
            AllowRefresh = true
        }, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("/google-response")]
    public async Task<IActionResult> HandleGoogleResponse()
    {
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (result.Principal != null)
        {
            var email = result.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(email))
                return RedirectToAction("Login", "Account");

            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                var userName = result.Principal.Claims.First(c => c.Type == ClaimTypes.GivenName).Value;

                var existsUser = await _userManager.FindByNameAsync(userName);
                if (existsUser is not null || userName.Length < DataSizes.ApplicationUserDataSizes.MinUsernameSize)
                    userName += $"{userName}_{Guid.NewGuid()}";

                user = new ApplicationUser
                {
                    UserName = userName,
                    Email = email,
                    EmailConfirmed = true,
                    IsPublic = false
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                    return RedirectToAction("Login", "Account");
            }
            else if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
            }

            await _signInManager.SignInAsync(user, true);

            return RedirectToAction("Index", "Home");
        }

        return RedirectToAction("Login", "Account");
    }
}