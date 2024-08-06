using System.Security.Claims;
using Domain.Enums;
using Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using WebUI.Policies.Requirements;

namespace WebUI.Policies.Handlers;

public class VerifyClaimsRequirementHandler : AuthorizationHandler<VerifyClaimsRequirement>
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;


    public VerifyClaimsRequirementHandler(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        VerifyClaimsRequirement requirement)
    {
        var idClaim = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(idClaim?.Value) || !Guid.TryParse(idClaim.Value, out var userId))
        {
            await _signInManager.SignOutAsync();
            context.Fail();
            return;
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            await _signInManager.SignOutAsync();
            context.Fail();
            return;
        }

        if (!user.EmailConfirmed)
        {
            await _signInManager.SignOutAsync();
            context.Fail();
            return;
        }

        var rolesFromClaim = context.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
        var roles = await _userManager.GetRolesAsync(user);

        if (Enum.GetNames<Role>().Any(role => rolesFromClaim.Contains(role) != roles.Contains(role)))
        {
            await _signInManager.SignOutAsync();
            context.Fail();
            return;
        }

        context.Succeed(requirement);
    }
}