using AutoMapper;
using Domain.Enums;
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
[Route("[controller]/[action]")]
[AllowAnonymous]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer<AccountController> _localizer;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IMapper mapper, IStringLocalizer<AccountController> localizer)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _mapper = mapper;
        _localizer = localizer;
    }
    
    [AllowAnonymous]
    [HttpGet("/register")]
    public IActionResult Register()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpPost("/register")]
    public async Task<IActionResult> Register([FromForm] RegisterDto registerDto, [FromServices] IEmailSender emailSender)
    {
        if (!ModelState.IsValid) return View(registerDto);

        var existsUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existsUser is not null)
        {
            if (existsUser.EmailConfirmed)
            {
                ModelState.AddModelError(nameof(registerDto.Email), "User with this email already exists");
                return View(registerDto);
            }

            var timeSpan = DateTime.UtcNow - existsUser.RegistrationDateTime;
            if (timeSpan.TotalHours < 3)
            {
                ModelState.AddModelError(nameof(registerDto.Email), "User with this email already exists");
                return View(registerDto);
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
        }
        else
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("Register", error.Description);

            return View(registerDto);
        }

        return this.InfoRedirect(100,
            ["Letter with confirmation was sent to your email", "Check it and confirm before login"]);
    }
    
    [AllowAnonymous]
    [HttpGet("/login")]
    public IActionResult Login()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpPost("/login")]
    public async Task<IActionResult> Login([FromForm] LoginDto loginDto, string? returnUrl)
    {
        if (!ModelState.IsValid) return View(loginDto);
        
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
            ModelState.AddModelError("Login", "Invalid login or password");
            return View(loginDto);
        }
        
        var result = await _signInManager.PasswordSignInAsync(user.UserName!, loginDto.Password!, isPersistent: true, lockoutOnFailure: false);
        
        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) 
                return LocalRedirect(returnUrl);
            return LocalRedirect("/");
        }

        if (user.EmailConfirmed)
        {
            ModelState.AddModelError("Login", "Invalid login or password");
        }
        else
        {
            ModelState.AddModelError("Login", "Your email is not confirmed. Check your mail and confirm it");
            ModelState.AddModelError("Login", "If you haven't this letter. If you have written correct email you can get new confirmation letter. Go to help and click to resend confirmation email letter");
        }
        
        return View(loginDto);
    }

    [AuthorizeVerifiedRoles]
    [Route("/logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return LocalRedirect("/");
    }
    
    [AuthorizeVerifiedRoles]
    [HttpGet("/profile")]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login","Account", new {ReturnUrl = "/profile"});

        var profile = _mapper.Map<ProfileViewModel>(user);
        profile.Roles = await _userManager.GetRolesAsync(user);
        return View(profile);
    }
    
    [AuthorizeVerifiedRoles]
    [HttpGet("/profile/update")]
    public async Task<IActionResult> UpdateProfile()
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user == null)
            return RedirectToAction("Login","Account", new {ReturnUrl = "/profile"});

        var updateProfileDto = _mapper.Map<UpdateProfileDto>(user);
        
        return View(updateProfileDto);
    }

    [AuthorizeVerifiedRoles]
    [HttpPost("/profile/update")]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto? updateProfileDto, [FromServices] IEmailSender emailSender)
    {
        ArgumentNullException.ThrowIfNull(updateProfileDto);

        if (!ModelState.IsValid)
            return View(updateProfileDto);
        
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user == null) return this.NotFoundRedirect(["User was not found"]);

        user.UserName = updateProfileDto.Username;

        bool logout = false;
        if (user.Email != updateProfileDto.Email)
        {
            user.EmailConfirmed = false;
            user.Email = updateProfileDto.Email;
            logout = true;
        }
            
        user.PhoneNumber = updateProfileDto.Phone;
        user.IsPublic = updateProfileDto.IsPublic;
        user.About = updateProfileDto.About;

        if (await _userManager.CheckPasswordAsync(user, updateProfileDto.CurrentPassword!))
        {
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return this.BadRequestRedirect(result.Errors.Select(err => err.Description));
            if (logout)
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

            if (!logout)
            {
                await _signInManager.RefreshSignInAsync(user);
                return RedirectToAction("Profile");
            }

            //await _signInManager.SignOutAsync();
            return this.InfoRedirect(100,
                ["Letter with confirmation was sent to your email", "Check it and confirm before login"]);
        }

        ModelState.AddModelError(nameof(updateProfileDto.CurrentPassword), "Password is not valid");
        return View(updateProfileDto);
    }
    
    [HttpPost]
    [AuthorizeVerifiedRoles]
    public async Task<IActionResult> DeleteAccount([FromForm] string? password)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user == null)
            return RedirectToAction("Login","Account");

        if (string.IsNullOrWhiteSpace(password)) return this.BadRequestRedirect(new[] { "Invalid password" });

        if (!await _userManager.CheckPasswordAsync(user, password))
            return this.BadRequestRedirect(new[] { "Invalid password" });
        
        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        return this.ErrorRedirect(500, result.Errors.Select(err => err.Description));
    }
    
    [AllowAnonymous]
    public async Task<IActionResult> IsEmailFree(string? email)
    {
        if (email == null)
        {
            return Json(false);
        }
        var user = await _userManager.FindByEmailAsync(email);
        return Json(user == null);
    }
    
    [AllowAnonymous]
    public async Task<IActionResult> IsUserNameFree(string? userName)
    {
        if (userName == null)
        {
            return Json(false);
        }
        var user = await _userManager.FindByNameAsync(userName);
        return Json(user == null);
    }

    [AllowAnonymous]
    [Route("/access-denied")]
    public IActionResult AccessDenied(string? returnUrl)
    {
        return this.ForbiddenRedirect([$"Access denied to url: {returnUrl}"]);
    }
}