
using System.ComponentModel.DataAnnotations;
using Contracts.Models;
using Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using SimpleAuthorizer;
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
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "Users");
        
        using var scope = _scopeFactory.CreateScope();
        var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
        var user = await usersCollectionService.Get(u => u.Id == userId);
        if (user == null) return View("Error", (404, "User was not found"));

        return View(new UserViewModel(user));
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
        var usersAuthorizer = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
        var user = await usersAuthorizer.Get(u => u.Email == model.Email);

        if (user == null)
        {
            ModelState.AddModelError(nameof(LoginUserModel.Email), "User was not found");
            return View(model);
        }

        if (user.Password != model.Password)
        {
            ModelState.AddModelError(nameof(LoginUserModel.Password), "Wrong password");
            return View(model);
        }

        HttpContext.AddIdToSession(user.Id);
        
        return RedirectToAction("Index", "Home");
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
            var usersAuthorizer = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
            await usersAuthorizer.Add(user);
            await usersAuthorizer.SaveChanges();

            HttpContext.AddIdToSession(user.Id);
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
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "Users");

        if (!ModelState.IsValid)
            return BadRequest(new
                {
                    message = string.Join('\n', 
                        ModelState.Values
                            .SelectMany(s => s.Errors)
                            .Select(e => e.ErrorMessage))
                });
        
        using var scope = _scopeFactory.CreateScope();
        var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
        var user = await usersCollectionService.Get(u => u.Id == userId);
        if (user == null) return NotFound(new { message = "User was not found" });
        if (user.Password != model.Password) return BadRequest(new { message = "Wrong password" });
        
        try
        {
            await usersCollectionService.Remove(userId.Value);
            await usersCollectionService.SaveChanges();
            HttpContext.RemoveIdFromSession();
            return Ok();
        }
        catch (Exception e)
        {
            return StatusCode(500, new {message = e.Message});
        }
    }

    [HttpGet]
    public IActionResult Edit()
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    public IActionResult Edit(int model)
    {
        throw new NotImplementedException();
    }

    [HttpPatch]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
    {
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "Users");

        if (!ModelState.IsValid)
            return StatusCode(400, 
                new
                {
                    message = string.Join('\n', 
                        ModelState.Values
                        .SelectMany(s => s.Errors)
                        .Select(e => e.ErrorMessage))
                });
        
        using var scope = _scopeFactory.CreateScope();
        var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
        try
        {
            await usersCollectionService.Update(userId.Value, u =>
            {
                if (u.Password != model.CurrentPassword)
                {
                    throw new Exception("Wrong current password");
                }

                u.Password = model.NewPassword;
            });
            await usersCollectionService.SaveChanges();
            return Ok();
        }
        catch (Exception e)
        {
            return StatusCode(500, new {message = e.Message});
        }
    }
}