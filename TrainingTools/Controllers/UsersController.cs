
using System.ComponentModel.DataAnnotations;
using Contracts.Exceptions;
using Contracts.Models;
using Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using TrainingTools.Models;

namespace TrainingTools.Controllers;

[Route("account/[action]")]
public class UsersController : Controller
{
    private readonly IServiceScopeFactory _scopeFactory;

    public UsersController(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try
        {
            if (!await authorizedUser.Authorize(HttpContext)) return RedirectToAction("Login", "Users");
        }
        catch (NotFoundException e)
        {
            return View("Error", (404, e.Message));
        }

        return View(new UserViewModel(authorizedUser.User)); // what should be authorizedUser.User or service.Get ???
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromForm] LoginUserModel model)
    {
        if (!ModelState.IsValid) return View(model);
        
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        
        try
        {
            await authorizedUser.Authorize(HttpContext, model.Email);
        }
        catch (NotFoundException e)
        {
            ModelState.AddModelError(nameof(LoginUserModel.Email), e.Message);
            return View(model);
        }
        
        if (!authorizedUser.ConfirmPassword(model.Password))
        {
            ModelState.AddModelError(nameof(LoginUserModel.Password), "Wrong password");
            return View(model);
        }
        
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Logout()
    {
        throw new NotImplementedException();
    }
    
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> Register([FromForm] RegisterUserModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            Email = model.Email,
            Password = model.Password
        };

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var usersService = scope.ServiceProvider.GetRequiredService<IUsersService>();
            await usersService.Add(user);
            
            var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
            await authorizedUser.Authorize(HttpContext, user.Email);
            await authorizedUser.SaveChanges();
            
            return RedirectToAction("Index", "Home");
        }
        catch (Exception)
        {
            ModelState.AddModelError(nameof(RegisterUserModel.Email), "Email already exists");
            return View(model);
        }
        // catch (Exception ex)
        // {
        //     ModelState.AddModelError("", "An error occurred while registering the user.");
        //     // Log the exception
        //     return View(model);
        // }
    }
    
    [HttpDelete]
    public async Task<IActionResult> Delete([Required, FromBody] DeleteUserModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(new
                {
                    message = string.Join('\n', 
                        ModelState.Values
                            .SelectMany(s => s.Errors)
                            .Select(e => e.ErrorMessage))
                });
        
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        
        try
        {
            await authorizedUser.Authorize(HttpContext);
            
            if (authorizedUser.ConfirmPassword(model.Password))
            {
                var usersService = scope.ServiceProvider.GetRequiredService<IUsersService>();
                await usersService.Remove(authorizedUser.User.Id);
                await authorizedUser.SaveChanges();
                authorizedUser.EndAuthorization(HttpContext);
            }
            
            return Ok();
        }
        catch (Exception e)
        {
            return StatusCode(500, new {message = e.Message});
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try
        {
            if (!await authorizedUser.Authorize(HttpContext)) return RedirectToAction("Login", "Users");
        }
        catch (NotFoundException e)
        {
            return View("Error", (404, e.Message));
        }

        return View(new EditUserModel{Name = authorizedUser.User.Name, Email = authorizedUser.User.Email});
    }

    [HttpPost]
    public async Task<IActionResult> Edit([Required, FromForm] EditUserModel model)
    {
        if (!ModelState.IsValid) return View(model);
        
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try
        {
            if (!await authorizedUser.Authorize(HttpContext)) return RedirectToAction("Login", "Users");
        }
        catch (NotFoundException e)
        {
            return View("Error", (404, e.Message));
        }

        if (!authorizedUser.ConfirmPassword(model.Password))
        {
            ModelState.AddModelError(nameof(LoginUserModel.Password), "Wrong password");
            return View(model);
        }

        var usersService = scope.ServiceProvider.GetRequiredService<IUsersService>();
        
        try
        {
            await usersService.Update(authorizedUser.User.Id, u =>
            {
                u.Email = model.Email;
                u.Name = model.Name;
            });
            await authorizedUser.SaveChanges();
        }
        catch (Exception e)
        {
            return View("Error", (500, e.Message));
        }

        return RedirectToAction("Index");
    }

    [HttpPatch]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(new
                {
                    message = string.Join('\n', 
                        ModelState.Values
                        .SelectMany(s => s.Errors)
                        .Select(e => e.ErrorMessage))
                });
        
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try
        {
            if (!await authorizedUser.Authorize(HttpContext)) return RedirectToAction("Login", "Users");
        }
        catch (NotFoundException e)
        {
            return BadRequest(new {message = e.Message});
        }

        if (!authorizedUser.ConfirmPassword(model.CurrentPassword))
        {
            return BadRequest(new { message = "Wrong password" });
        }

        var usersService = scope.ServiceProvider.GetRequiredService<IUsersService>();
        try
        {
            await usersService.Update(authorizedUser.User.Id, u =>
            {
                u.Password = model.NewPassword;
            });
            await authorizedUser.SaveChanges();
            return Ok();
        }
        catch (Exception e)
        {
            return StatusCode(500, new {message = e.Message});
        }
    }
}