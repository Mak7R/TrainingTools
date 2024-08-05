using Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WebUI.Extensions;

namespace WebUI.Controllers;

[Controller]
[AllowAnonymous]
public class EmailConfirmationController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStringLocalizer<AccountController> _localizer;

    public EmailConfirmationController(UserManager<ApplicationUser> userManager, IStringLocalizer<AccountController> localizer)
    {
        _userManager = userManager;
        _localizer = localizer;
    }

    [HttpPost("/resend-email-confirmation")]
    public async Task<IActionResult> ResendEmailConfirmationLetter(string email, [FromServices] IEmailSender emailSender)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return this.BadRequestRedirect(["User was not registered"]);

        if (user.EmailConfirmed)
            return this.BadRequestRedirect(["Email already confirmed"]);
        
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = Url.Action("ConfirmEmail", "EmailConfirmation", new { userId = user.Id, token }, Request.Scheme) ?? $"?userId{user.Id}&token={token}";
        await emailSender.SendEmailAsync(email, _localizer["ConfirmEmailTitle"], _localizer["ConfirmEmailText", confirmationLink]);

        return this.InfoRedirect(100,
            ["Letter with confirmation was sent to your email", "Check it and confirm before login"]);
    }
    
    [HttpGet("/account/confirm-email")]
    public async Task<IActionResult> ConfirmEmail(Guid userId, string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction("Index", "Home");

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return RedirectToAction("Login","Account");

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
            return this.InfoRedirect(200, ["Email was confirmed successful", "Now you can log in your account"]);

        return this.ErrorRedirect(500, ["Unexpected error was occured while processing the request"]);
    }
}