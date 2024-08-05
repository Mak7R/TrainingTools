using System.Security.Claims;
using Application.Interfaces.Services;
using Application.Models;
using Domain.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebUI.Models.Api.Account;

namespace WebUI.Controllers.Api.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
[AllowAnonymous]
public class GoogleAuthenticationController : ApiController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuthTokenService<TokenGenerationInfo> _tokenService;

    public GoogleAuthenticationController(UserManager<ApplicationUser> userManager, IAuthTokenService<TokenGenerationInfo> tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }
    
    /// <summary>
    /// Authenticate user in application by Google JWT Access Token
    /// </summary>
    /// <returns>authentication info with jwt access token of type <see cref="AuthenticationResponse"/></returns>
    [HttpGet("google-response")]
    public async Task<IActionResult> HandleGoogleResponse()
    {
        var result = await HttpContext.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
        if (result.Principal != null)
        {
            var email = result.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(email))
                return Problem("Email was empty", statusCode:400);
            
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                var userName = result.Principal.Claims.First(c => c.Type == ClaimTypes.GivenName).Value;

                var existsUser = await _userManager.FindByNameAsync(userName);
                if (existsUser is not null || userName.Length < Domain.Rules.DataSizes.ApplicationUserDataSizes.MinUsernameSize)
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
                    return StatusCode(500, new ProblemDetails
                    {
                        Detail = "Register account was not successful",
                        Status = 500,
                        Extensions = new Dictionary<string, object?>
                        {
                            { "errors", createResult.Errors.Select(e => e.Description) }
                        }
                    });
            }
            else if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
            }
            
            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.GenerateToken(new TokenGenerationInfo { User = user, Roles = roles });
            return Ok(new AuthenticationResponse{UserName = user.UserName, Email = user.Email, Roles = roles, Token = token});
        }
        return Problem("Authentication data was not found in request", statusCode:400);
    }
}