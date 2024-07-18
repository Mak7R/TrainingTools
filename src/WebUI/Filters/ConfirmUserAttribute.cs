using System.Security.Claims;
using Domain.Enums;
using Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebUI.Controllers;

namespace WebUI.Filters;

/// <summary>
/// Confirm user is confirming is user really exists in database and has rights as it is in identity claims.
/// Recommends to user this attribute for actions which can provide some secret data or can change data and actions is accessible only for some user roles. 
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class ConfirmUserAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.HttpContext.User.Identity is not ClaimsIdentity identity) 
            return;
        
        var userId = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return;

        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(userId);

        if (user is null) // check is user deleted
        {
            await Invalid(context);
            return;
        }
            
        var roles = await userManager.GetRolesAsync(user); // check does user have the same roles
        if (Enum.GetNames<Role>().Any(role => context.HttpContext.User.IsInRole(role) != roles.Contains(role)))
        {
            await Invalid(context);
        }
    }

    private static async Task Invalid(AuthorizationFilterContext context)
    {
        var signInManager = context.HttpContext.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();
        await signInManager.SignOutAsync();

        if (context.HttpContext.Request.Path.StartsWithSegments("/api"))
        {
            context.Result = new UnauthorizedResult();
        }
        else
        {
            context.Result =
                new RedirectToActionResult(nameof(AccountsController.Login), "Accounts", null);
        }
    }
}