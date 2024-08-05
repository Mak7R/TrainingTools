using System.ComponentModel.DataAnnotations;
using Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WebUI.Extensions;
using WebUI.Filters;
using WebUI.Models.Account;

namespace WebUI.Controllers;

[Controller]
[Route("/password")]
[AllowAnonymous]
public class PasswordsController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStringLocalizer<PasswordsController> _localizer;

    public PasswordsController(UserManager<ApplicationUser> userManager, IStringLocalizer<PasswordsController> localizer)
    {
        _userManager = userManager;
        _localizer = localizer;
    }
    
    [AllowAnonymous]
    [HttpGet("/forgot-password")]
    public IActionResult Forgot()
    {
        return View();
    }
    
    [AllowAnonymous]
    [HttpPost("/forgot-password")]
    public async Task<IActionResult> Forgot([EmailAddress] string email, [FromServices] IEmailSender emailSender)
    {
        if (!ModelState.IsValid)
            return View(email);

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            ModelState.AddModelError(nameof(email), "User was not found");
            return View(email);
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var reference = Url.Action("Reset", "Passwords", new { email, token }, Request.Scheme) ?? $"?email={email}&token={token}";
        await emailSender.SendEmailAsync(email, _localizer["ResetPasswordTitle"], _localizer["ResetPasswordText", reference]);

        return this.InfoRedirect(100, ["Link for updating password was successfully sent to your email. Check it and follow link."]);
    }

    [AllowAnonymous]
    [HttpGet("/reset-password")]
    public IActionResult Reset([EmailAddress] string email, string token)
    {
        if (!ModelState.IsValid)
            return this.BadRequestRedirect(["Model state is not valid"]);

        return View(new ResetPasswordModel { Email = email, Token = token, NewPassword = string.Empty });
    }

    [HttpPost("/reset-password")]
    public async Task<IActionResult> Reset(ResetPasswordModel resetPasswordModel)
    {
        if (!ModelState.IsValid)
            return View(resetPasswordModel);
        
        var user = await _userManager.FindByEmailAsync(resetPasswordModel.Email);
        if (user is null)
        {
            ModelState.AddModelError(nameof(ResetPasswordModel.Email), "User was not found");
            return View(resetPasswordModel);
        }

        var result = await _userManager.ResetPasswordAsync(user, resetPasswordModel.Token, resetPasswordModel.NewPassword);

        if (result.Succeeded)
            return RedirectToAction("Login", "Account");
        
        foreach (var error in result.Errors)
            ModelState.AddModelError(nameof(ResetPasswordModel), error.Description);
        resetPasswordModel.NewPassword = string.Empty;
        return View(resetPasswordModel);
    }

    [AuthorizeVerifiedRoles]
    [HttpGet("/change-password")]
    public IActionResult Change()
    {
        return View();
    }
    
    [AuthorizeVerifiedRoles]
    [HttpPost("/change-password")]
    public async Task<IActionResult> Change(ChangePasswordModel changePasswordModel)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return RedirectToAction("Login", "Account");
        
        var result = await _userManager.ChangePasswordAsync(user, changePasswordModel.OldPassword, changePasswordModel.Password);

        if (result.Succeeded)
            return RedirectToAction("Index", "Home");
        
        foreach (var error in result.Errors)
            ModelState.AddModelError(nameof(ResetPasswordModel), error.Description);
        return View(changePasswordModel);
    }
}