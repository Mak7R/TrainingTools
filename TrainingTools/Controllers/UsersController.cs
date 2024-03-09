
using System.ComponentModel.DataAnnotations;
using Contracts.Exceptions;
using Contracts.Models;
using Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using Services.Client;
using TrainingTools.Models;
using TrainingTools.ViewModels;

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
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorModel(e.Message)); }
        
        return Json(new UserViewModel(authorizedUser.User)); // what should be authorizedUser.User or service.Get ???
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginUserModel model)
    {
        if (!ModelState.IsValid) return BadRequest(new ErrorModel("Model is not valid"));
        
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try {
            if (!await authorizedUser.Authorize(HttpContext, model.Email, model.Password))
            {
                return BadRequest(new ErrorModel("Wrong email or password"));
            } 
        }
        catch (NotFoundException e) { return NotFound(new ErrorModel(e.Message)); }
        
        return Ok();
    }

    [HttpGet]
    public IActionResult Logout()
    {
        throw new NotImplementedException();
    }
    
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterUserModel model)
    {
        if (!ModelState.IsValid) return BadRequest(new ErrorModel("Model is not valid"));

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
            await authorizedUser.SaveChanges();
            await authorizedUser.Authorize(HttpContext, user.Email, user.Password);

            return Ok();
        }
        catch (Exception)
        {
            // email doesnt exist
            return BadRequest(new ErrorModel("Email already exist"));
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
            return BadRequest(
                new ErrorModel(
                    string.Join(
                        '\n', 
                        ModelState.Values
                            .SelectMany(s => s.Errors)
                            .Select(e => e.ErrorMessage)
                    )
                )
            );
        
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
            return StatusCode(500, new ErrorModel(e.Message));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Edit([Required, FromBody] EditUserModel model)
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