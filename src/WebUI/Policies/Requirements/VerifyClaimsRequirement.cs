using Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace WebUI.Policies.Requirements;

public class VerifyClaimsRequirement : IAuthorizationRequirement
{
}