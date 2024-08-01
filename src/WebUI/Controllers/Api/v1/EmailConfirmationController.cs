using Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace WebUI.Controllers.Api.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
[AllowAnonymous]
public class EmailConfirmationController : ApiController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStringLocalizer<Controllers.AccountController> _localizer;

    public EmailConfirmationController(UserManager<ApplicationUser> userManager, IStringLocalizer<Controllers.AccountController> localizer)
    {
        _userManager = userManager;
        _localizer = localizer;
    }

    [HttpPost("resend-email-confirmation")]
    public async Task<IActionResult> ResendEmailConfirmation(string email, [FromServices] IEmailSender emailSender)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return Problem("User was not registered", statusCode: 400);

        if (user.EmailConfirmed)
            return Problem("Email already confirmed", statusCode: 400);
        
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = Url.Action("ConfirmEmail", "EmailConfirmation", new { userId = user.Id, token }, Request.Scheme) ?? $"?userId{user.Id}&token={token}";
        await emailSender.SendEmailAsync(email, _localizer["ConfirmEmailTitle"], _localizer["ConfirmEmailText", confirmationLink]);

        return Ok(email);
    }
    
    [HttpGet("account/confirm-email")]
    public async Task<IActionResult> ConfirmEmail(Guid userId, string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction("Index", "Home");

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return RedirectToAction("Login","Account");

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
            return Ok(userId);

        return Problem("Unexpected error was occured while processing the request", statusCode:500);
    }
}