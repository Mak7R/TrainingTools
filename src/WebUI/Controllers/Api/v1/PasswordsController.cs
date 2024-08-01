using System.ComponentModel.DataAnnotations;
using Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WebUI.Filters;
using WebUI.Models.Account;

namespace WebUI.Controllers.Api.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
[AllowAnonymous]
public class PasswordsController : ApiController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStringLocalizer<Controllers.PasswordsController> _localizer;

    public PasswordsController(UserManager<ApplicationUser> userManager, IStringLocalizer<Controllers.PasswordsController> localizer)
    {
        _userManager = userManager;
        _localizer = localizer;
    }
    
    [AllowAnonymous]
    [HttpPost("/forgot-password")]
    public async Task<IActionResult> Forgot([EmailAddress] string email, [FromServices] IEmailSender emailSender)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return Problem("User with this email was not found", statusCode: 404);

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var reference = Url.Action("Reset", "Passwords", new { email, token }, Request.Scheme) ?? $"?email={email}&token={token}";
        await emailSender.SendEmailAsync(email, _localizer["ResetPasswordTitle"], _localizer["ResetPasswordText", reference]);

        return Ok(email);
    }

    [HttpPost("/reset-password")]
    public async Task<IActionResult> Reset(ResetPasswordModel resetPasswordModel)
    {
        var user = await _userManager.FindByEmailAsync(resetPasswordModel.Email);
        if (user is null)
            return Problem("User with this email was not found", statusCode: 404);

        var result = await _userManager.ResetPasswordAsync(user, resetPasswordModel.Token, resetPasswordModel.NewPassword);

        if (result.Succeeded)
            Ok();
        
        foreach (var error in result.Errors)
            ModelState.AddModelError(nameof(ResetPasswordModel), error.Description);
        resetPasswordModel.NewPassword = string.Empty;
        return BadRequest(new ProblemDetails
        {
            Detail = "Reset password was not successful",
            Status = 400,
            Extensions = new Dictionary<string, object?>
            {
                { "errors", result.Errors.Select(e => e.Description) }
            }
        });
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
        
        return BadRequest(new ProblemDetails
        {
            Detail = "Password was not changed",
            Status = 400,
            Extensions = new Dictionary<string, object?>
            {
                {"errors", result.Errors.Select(e => e.Description)}
            }
        });
    }
}