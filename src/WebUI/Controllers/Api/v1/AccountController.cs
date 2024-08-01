using Application.Interfaces.Services;
using Application.Models;
using AutoMapper;
using Domain.Enums;
using Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WebUI.Filters;
using WebUI.Models.Account;
using WebUI.Models.Api.Account;

namespace WebUI.Controllers.Api.v1;

[ApiVersion("1.0")]
public class AccountController : ApiController
{
    private readonly IAuthTokenService<TokenGenerationInfo> _tokenService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer<Controllers.AccountController> _localizer;

    public AccountController(UserManager<ApplicationUser> userManager, IMapper mapper, IAuthTokenService<TokenGenerationInfo> tokenService, IStringLocalizer<Controllers.AccountController> localizer)
    {
        _userManager = userManager;
        _mapper = mapper;
        _tokenService = tokenService;
        _localizer = localizer;
    }
    
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto registerDto, [FromServices] IEmailSender emailSender)
    {
        var existsUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existsUser is not null)
        {
            if (existsUser.EmailConfirmed)
            {
                return Problem("User with this email already exists", statusCode: 400, title: "User with this email already exists");
            }

            var timeSpan = DateTime.UtcNow - existsUser.RegistrationDateTime;
            if (timeSpan.TotalHours < 3)
            {
                return Problem("User with this email already exists", statusCode: 400, title: "User with this email already exists");
            }

            await _userManager.DeleteAsync(existsUser);
        }
        
        var newUser = _mapper.Map<ApplicationUser>(registerDto);
        
        var result = await _userManager.CreateAsync(newUser, registerDto.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(newUser, nameof(Role.User));
            
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
            var confirmationLink = Url.Action("ConfirmEmail", "EmailConfirmation", new { userId = newUser.Id, token }, Request.Scheme) ?? $"?userId{newUser.Id}&token={token}";
            await emailSender.SendEmailAsync(registerDto.Email, _localizer["ConfirmEmailTitle"], _localizer["ConfirmEmailText", confirmationLink]);
            
            return Ok(new AuthenticationResponse{UserName = newUser.UserName, Email = newUser.Email, Roles = [nameof(Role.User)]});
        }
        return StatusCode(500,
            new ProblemDetails
            {
                Detail = "Error was occured while processing the request",
                Extensions = new Dictionary<string, object?>
                {
                    {"errors", result.Errors}
                },
                Status = 500,
                Title = "Server error"
            });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        ApplicationUser? user;

        if (loginDto.EmailOrUsername!.Contains('@'))
        {
            user = await _userManager.FindByEmailAsync(loginDto.EmailOrUsername!);
        }
        else
        {
            user = await _userManager.FindByNameAsync(loginDto.EmailOrUsername);
        }
        
        if (user is null)
        {
            return Problem(detail:"Invalid login or password", statusCode:400);
        }

        if (!user.EmailConfirmed)
        {
            return Problem(detail: "Email is not confirmed", statusCode: StatusCodes.Status403Forbidden,
                title: "Email is not confirmed");
        }
        
        if (await _userManager.CheckPasswordAsync(user, loginDto.Password!))
        {
            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.GenerateToken(new TokenGenerationInfo { User = user, Roles = roles });
            return Ok(new AuthenticationResponse{UserName = user.UserName, Email = user.Email, Roles = roles, Token = token});
        }
        
        return Problem(detail:"Invalid login or password", statusCode:400);
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Problem("User unauthorized", statusCode:StatusCodes.Status401Unauthorized);

        var profile = _mapper.Map<ProfileViewModel>(user);
        profile.Roles = await _userManager.GetRolesAsync(user);

        return Ok(profile);
    }
    

    [AuthorizeVerifiedRoles]
    [HttpPost("profile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileDto? updateProfileDto, [FromServices] IEmailSender emailSender)
    {
        ArgumentNullException.ThrowIfNull(updateProfileDto);
        
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user == null) return Problem("User unauthorized", statusCode:StatusCodes.Status401Unauthorized);

        user.UserName = updateProfileDto.Username;

        bool emailUpdated = false;
        if (user.Email != updateProfileDto.Email)
        {
            user.EmailConfirmed = false;
            user.Email = updateProfileDto.Email;
            emailUpdated = true;
        }
        
        user.PhoneNumber = updateProfileDto.Phone;
        user.IsPublic = updateProfileDto.IsPublic;
        user.About = updateProfileDto.About;

        if (await _userManager.CheckPasswordAsync(user, updateProfileDto.CurrentPassword!))
        {
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) 
                return StatusCode(400, new ProblemDetails{Detail = "Update user error", Extensions = new Dictionary<string, object?>{{"error", result.Errors.Select(err=>err.Description)}}});

            if (emailUpdated)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action("ConfirmEmail", "EmailConfirmation", new { userId = user.Id, token }, Request.Scheme) ?? $"?userId{user.Id}&token={token}";
                await emailSender.SendEmailAsync(updateProfileDto.Email!, _localizer["ConfirmEmailTitle"], _localizer["ConfirmEmailText", confirmationLink]);
            }
            
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(nameof(Role.Admin)) || roles.Contains(nameof(Role.Root)))
            {
                var isTrainer = roles.Contains(nameof(Role.Trainer));
                if (!isTrainer && updateProfileDto.IsTrainer)
                {
                    await _userManager.AddToRoleAsync(user, nameof(Role.Trainer));
                }
                else if (isTrainer && !updateProfileDto.IsTrainer)
                {
                    await _userManager.RemoveFromRoleAsync(user, nameof(Role.Trainer));
                }
            }
            
            return RedirectToAction("Profile");
        }

        return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails { Detail = "Invalid password", Status = 403});
    }
    
    [HttpDelete]
    [AuthorizeVerifiedRoles]
    public async Task<IActionResult> DeleteAccount([FromBody] string? password)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user == null)
            return Problem("User unauthorized", statusCode:StatusCodes.Status401Unauthorized);

        if (string.IsNullOrWhiteSpace(password)) 
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails { Detail = "Invalid password", Status = 403});

        if (!await _userManager.CheckPasswordAsync(user, password))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails { Detail = "Invalid password", Status = 403});
        }
        
        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            return RedirectToAction("Index", "Home");
        }
        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        {
            Detail = "Delete user error", Status = 500, 
            Extensions = new Dictionary<string, object?>{{"errors", result.Errors.Select(err => err.Description)}}
        });
    }
    
    [AllowAnonymous]
    [HttpGet("is-email-free")]
    public async Task<IActionResult> IsEmailFree(string? email)
    {
        if (email == null)
        {
            return Problem("Email is empty", statusCode: 400, title: "Email is empty");
        }
        var user = await _userManager.FindByEmailAsync(email);
        return Ok(user == null);
    }
    
    [AllowAnonymous]
    [HttpGet("is-username-free")]
    public async Task<IActionResult> IsUserNameFree(string? userName)
    {
        if (userName == null)
        {
            return Problem("Username is empty", statusCode: 400, title: "Username is empty");
        }
        var user = await _userManager.FindByNameAsync(userName);
        return Ok(user == null);
    }
}