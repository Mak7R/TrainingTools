using Application.Identity;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        var profile = new FullProfileViewModel
        {
            Username = user.UserName,
            About = user.About,
            Email = user.Email,
            Phone = user.PhoneNumber,
            Roles = userRoles
        };
        return View(profile);
    }
    
    [AllowAnonymous]
    [HttpGet("/user/{userName}")]
    public async Task<IActionResult> PublicProfile(string userName) // TODO MUST BE REMOVED
    {
        if (User.Identity?.Name == userName)
            return RedirectToAction("Profile");
        
        var user = await userManager.FindByNameAsync(userName);

        if (user == null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NotFound", "User was not found");
        }
        
        var userRoles = await userManager.GetRolesAsync(user);
        
        var userProfile = new ProfileViewModel
        {
            Username = user.UserName,
            About = user.About,
            Roles = userRoles
        };
        
        return View(userProfile);
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