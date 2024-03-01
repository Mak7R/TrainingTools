
using Contracts.Models;
using Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using SimpleAuthorizer;
using TrainingTools.Models;

namespace TrainingTools.Controllers;

[Route("[action]")]
public class UsersController : Controller
{
    private readonly IServiceScopeFactory _scopeFactory;

    public UsersController(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
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

    [HttpGet]
    [ActionName("DeleteAccount")]
    public IActionResult Delete()
    {
        throw new NotImplementedException();
    }
    
    [HttpPost]
    [ActionName("DeleteAccount")]
    public IActionResult Delete(int model)
    {
        throw new NotImplementedException();
    }

    [HttpGet]
    [ActionName("EditAccount")]
    public IActionResult Edit()
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [ActionName("EditAccount")]
    public IActionResult Edit(int model)
    {
        throw new NotImplementedException();
    }
}