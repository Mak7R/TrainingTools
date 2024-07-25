using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using WebUI.Policies.Requirements;

namespace WebUI.Filters;


public class AuthorizeVerifiedRolesAttribute : AuthorizeAttribute
{
    public AuthorizeVerifiedRolesAttribute(params Role[] roles) : base(nameof(VerifyClaimsRequirement))
    {
        if (roles.Length != 0)
        {
            Roles = string.Join(",", roles);
        }
        
    }
}