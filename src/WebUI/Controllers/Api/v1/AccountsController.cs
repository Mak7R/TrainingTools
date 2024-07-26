using Application.Interfaces.Services;
using Application.Models;
using AutoMapper;
using Domain.Enums;
using Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebUI.Filters;
using WebUI.Models.Account;
using WebUI.Models.Api.Account;

namespace WebUI.Controllers.Api.v1;

[ApiVersion("1.0")]
public class AccountsController : ApiController
{
    private readonly IAuthTokenService<TokenGenerationInfo> _tokenService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IMapper _mapper;

    public AccountsController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IMapper mapper, IAuthTokenService<TokenGenerationInfo> tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _mapper = mapper;
        _tokenService = tokenService;
    }
    
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        var newUser = _mapper.Map<ApplicationUser>(registerDto);

        var result = await _userManager.CreateAsync(newUser, registerDto.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(newUser, nameof(Role.User));
            //await _signInManager.SignInAsync(newUser, isPersistent: true);

            var token = _tokenService.GenerateToken(new TokenGenerationInfo { User = newUser, Roles = [nameof(Role.User)] });
            return Ok(new AuthenticationResponse{UserName = newUser.UserName, Email = newUser.Email, Roles = [nameof(Role.User)], Token = token});
        }
        return StatusCode(500,
            new ProblemDetails
            {
                Detail = "Error was occuranced while processing the request",
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
        
        //var result = await _signInManager.PasswordSignInAsync(user.UserName!, loginDto.Password!, isPersistent: true, lockoutOnFailure: false);
        
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
        if (user == null) return Problem("User was unauthorized", statusCode:StatusCodes.Status401Unauthorized);

        var profile = _mapper.Map<ProfileViewModel>(user);
        profile.Roles = await _userManager.GetRolesAsync(user);

        return Ok(profile);
    }
    

    [AuthorizeVerifiedRoles]
    [HttpPost("profile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileDto? updateProfileDto)
    {
        ArgumentNullException.ThrowIfNull(updateProfileDto);
        
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user == null) return Problem("User was unauthorized", statusCode:StatusCodes.Status401Unauthorized);

        user.UserName = updateProfileDto.Username;
        user.Email = updateProfileDto.Email;
        user.PhoneNumber = updateProfileDto.Phone;
        user.IsPublic = updateProfileDto.IsPublic;
        user.About = updateProfileDto.About;

        if (await _userManager.CheckPasswordAsync(user, updateProfileDto.CurrentPassword!))
        {
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) 
                return StatusCode(400, new ProblemDetails{Detail = "Update user error", Extensions = new Dictionary<string, object?>{{"error", result.Errors.Select(err=>err.Description)}}});
            
            if (!string.IsNullOrWhiteSpace(updateProfileDto.NewPassword))
            {
                var updatePasswordResult = await _userManager.ChangePasswordAsync(user, updateProfileDto.CurrentPassword!, updateProfileDto.NewPassword);
                if (!updatePasswordResult.Succeeded) 
                    return StatusCode(400, new ProblemDetails{Detail = "Update user password error", Extensions = new Dictionary<string, object?>{{"error", result.Errors.Select(err=>err.Description)}}});
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

            await _signInManager.RefreshSignInAsync(user);
            
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
            return RedirectToAction("Login","Accounts");

        if (string.IsNullOrWhiteSpace(password)) 
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails { Detail = "Invalid password", Status = 403});

        if (!await _userManager.CheckPasswordAsync(user, password))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails { Detail = "Invalid password", Status = 403});
        }
        
        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Detail = "Delete user error", Status = 500, Extensions = new Dictionary<string, object?>{{"errors", result.Errors.Select(err => err.Description)}}});
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