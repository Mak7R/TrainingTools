using Application.Identity;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebUI.Extensions;
using WebUI.Models.AccountModels;

namespace WebUI.Controllers;

[Controller]
[Route("[controller]/[action]")]
public class AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager)
    : Controller
{
    [AllowAnonymous]
    [HttpGet("/register")]
    public IActionResult Register()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpPost("/register")]
    public async Task<IActionResult> Register([FromForm] RegisterDto registerDto, string? returnUrl)
    {
        if (!ModelState.IsValid) return View(registerDto);

        var newUser = new ApplicationUser
        {
            UserName = registerDto.Username,
            Email = registerDto.Email,
            PhoneNumber = registerDto.Phone,
            About = registerDto.About,
            IsPublic = registerDto.IsPublic
        };

        var result = await userManager.CreateAsync(newUser, registerDto.Password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(newUser, nameof(Role.User));
            await signInManager.SignInAsync(newUser, isPersistent: true);
        }
        else
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("Register", error.Description);

            return View(registerDto);
        }
        
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) 
            return LocalRedirect(returnUrl);
        return RedirectToAction("Index", "Home");
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
            user = await userManager.FindByEmailAsync(loginDto.EmailOrUsername!);
        }
        else
        {
            user = await userManager.FindByNameAsync(loginDto.EmailOrUsername);
        }
        
        if (user is null)
        {
            ModelState.AddModelError("Login", "Invalid login or password");
            return View(loginDto);
        }
        
        var result = await signInManager.PasswordSignInAsync(user.UserName!, loginDto.Password!, isPersistent: true, lockoutOnFailure: false);
        
        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) 
                return LocalRedirect(returnUrl);
            return LocalRedirect("/");
        }
        
        ModelState.AddModelError("Login", "Invalid login or password");
        return View(loginDto);
    }

    [Authorize]
    [Route("/logout")]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return LocalRedirect("/");
    }

    [Authorize]
    [HttpGet("/profile")]
    public async Task<IActionResult> Profile()
    {
        var user = await userManager.GetUserAsync(User);
        
        if (user == null)
        {
            return RedirectToAction("Login","Account", new {ReturnUrl = "/profile"});
        }
        
        var userRoles = await userManager.GetRolesAsync(user);

        var profile = new ProfileViewModel
        {
            Username = user.UserName,
            About = user.About,
            Email = user.Email,
            Phone = user.PhoneNumber,
            Roles = userRoles
        };
        return View(profile);
    }
    
    [Authorize]
    [HttpGet("/profile/update")]
    public async Task<IActionResult> UpdateProfile()
    {
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user == null) return this.NotFoundView("User was not found");

        var updateProfileDto = new UpdateProfileDto
        {
            Username = user.UserName,
            Email = user.Email,
            Phone = user.PhoneNumber,
            About = user.About,
            IsPublic = user.IsPublic
        };
        
        return View(updateProfileDto);
    }
    
    [Authorize]
    [HttpPost("/profile/update")]
    public async Task<IActionResult> UpdateProfile([FromForm]UpdateProfileDto? updateProfileDto)
    {
        ArgumentNullException.ThrowIfNull(updateProfileDto);
        ArgumentException.ThrowIfNullOrWhiteSpace(updateProfileDto.CurrentPassword);
        
        if (!ModelState.IsValid)
        {
            return this.BadRequestView(ModelState.Values.SelectMany(v => v.Errors).Select(err=> err.ErrorMessage));
        }
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user == null) return this.NotFoundView("User was not found");

        user.UserName = updateProfileDto.Username;
        user.Email = updateProfileDto.Email;
        user.PhoneNumber = updateProfileDto.Phone;
        user.IsPublic = updateProfileDto.IsPublic;
        user.About = updateProfileDto.About;

        if (await userManager.CheckPasswordAsync(user, updateProfileDto.CurrentPassword))
        {
            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded) return this.BadRequestView(result.Errors.Select(err=>err.Description));

            if (!string.IsNullOrWhiteSpace(updateProfileDto.NewPassword))
            {
                var updatePasswordResult = await userManager.ChangePasswordAsync(user, updateProfileDto.CurrentPassword, updateProfileDto.NewPassword);
                if (!updatePasswordResult.Succeeded) return this.BadRequestView(updatePasswordResult.Errors.Select(err=>err.Description));
            }

            return RedirectToAction("Profile");
        }

        return this.BadRequestView(new []{"Current password is not valid"});
    }
    
    [AllowAnonymous]
    public async Task<IActionResult> IsEmailFree(string? email)
    {
        if (email == null)
        {
            return Json(false);
        }
        var user = await userManager.FindByEmailAsync(email);
        return Json(user == null);
    }
    
    [AllowAnonymous]
    public async Task<IActionResult> IsUserNameFree(string? userName)
    {
        if (userName == null)
        {
            return Json(false);
        }
        var user = await userManager.FindByNameAsync(userName);
        return Json(user == null);
    }

    [AllowAnonymous]
    [Route("")]
    public IActionResult AccessDenied(string? returnUrl)
    {
        return View("AccessDenied", returnUrl);
    }
}