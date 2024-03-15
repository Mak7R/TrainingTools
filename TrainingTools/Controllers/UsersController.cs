using Contracts.Exceptions;
using Contracts.Models;
using Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using TrainingTools.Extensions;
using TrainingTools.ViewModels;

namespace TrainingTools.Controllers;

[Route("api/v1/[controller]")]
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
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }
        
        return Json(new UserViewModel(authorizedUser.User)); // what should be authorizedUser.User or service.Get ???
    }
    
    [HttpGet("[action]")]
    public async Task<IActionResult> Logout()
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }
        
        authorizedUser.EndAuthorization(HttpContext);
        return Ok();
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> Login([FromBody] LoginUserModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState.ToModelStateErrorViewModel());
        
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try {
            if (!await authorizedUser.Authorize(HttpContext, model.Email, model.Password))
            {
                ModelState.AddModelError(nameof(LoginUserModel.Email), "Wrong email or password");
                ModelState.AddModelError(nameof(LoginUserModel.Password), "Wrong email or password");
                return BadRequest(ModelState.ToModelStateErrorViewModel());
            } 
        }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }
        
        return Ok();
    }
    
    [HttpPost("[action]")]
    public async Task<IActionResult> Register([FromBody] RegisterUserModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState.ToModelStateErrorViewModel());

        var user = new User
        {
            Name = model.Name,
            Email = model.Email,
            Password = model.Password
        };

        using var scope = _scopeFactory.CreateScope();
        var usersService = scope.ServiceProvider.GetRequiredService<IUsersService>();
        
        try
        {
            await usersService.Add(user);
        }
        catch (AlreadyExistsException)
        {
            ModelState.AddModelError(nameof(RegisterUserModel.Email), "User with this email already exists");
            return BadRequest(ModelState.ToModelStateErrorViewModel());
        }
        
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        await authorizedUser.SaveChanges();
        await authorizedUser.Authorize(HttpContext, user.Email, user.Password);

        return Ok();
    }
    
    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] DeleteUserModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState.ToModelStateErrorViewModel());
        
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }
        
        if (authorizedUser.ConfirmPassword(model.Password))
        {
            var usersService = scope.ServiceProvider.GetRequiredService<IUsersService>();
            await usersService.Remove(authorizedUser.User.Id);
            await authorizedUser.SaveChanges();
            authorizedUser.EndAuthorization(HttpContext);
            
            return Ok();
        }
        
        ModelState.AddModelError(nameof(DeleteUserModel.Password), "Wrong password");
        return BadRequest(ModelState.ToModelStateErrorViewModel());
    }

    [HttpPatch]
    public async Task<IActionResult> Edit([FromBody] EditUserModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState.ToModelStateErrorViewModel());
        
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }

        if (!authorizedUser.ConfirmPassword(model.Password))
        {
            ModelState.AddModelError(nameof(EditUserModel.Password), "Wrong password");
            return BadRequest(ModelState.ToModelStateErrorViewModel());
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
        catch (Exception)
        {
            ModelState.AddModelError(nameof(EditUserModel.Email), "Email already exist");
            return BadRequest(ModelState.ToModelStateErrorViewModel());
        }

        return Ok();
    }

    [HttpPatch("[action]")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState.ToModelStateErrorViewModel());
        
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }

        if (!authorizedUser.ConfirmPassword(model.CurrentPassword))
        {
            ModelState.AddModelError(nameof(ChangePasswordModel.CurrentPassword), "Wrong password");
            return BadRequest(ModelState.ToModelStateErrorViewModel());
        }

        var usersService = scope.ServiceProvider.GetRequiredService<IUsersService>();
        await usersService.Update(authorizedUser.User.Id, u =>
        {
            u.Password = model.NewPassword;
        });
        await authorizedUser.SaveChanges();
        return Ok();
    }
}