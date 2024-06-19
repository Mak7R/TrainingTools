using Application.Interfaces.ServiceInterfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Extensions;
using WebUI.Models.UserModels;

namespace WebUI.Controllers;


[Controller]
[Route("users")]
[Authorize]
public class UsersController : Controller
{
    private readonly IUsersService _usersService;

    public UsersController(IUsersService usersService)
    {
        _usersService = usersService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var userInfos = await _usersService.GetAllUsers(HttpContext.User);

        return View(userInfos);
    }

    [HttpGet("{userName}")]
    public async Task<IActionResult> GetUser(string userName)
    {
        var userInfo = await _usersService.GetByName(HttpContext.User, userName);
        // TODO check if current user is searchable user redirect to profile page
        if (userInfo is null) return this.NotFoundView("User with this name was not found");

        return View(userInfo);
    }
    
    [Authorize(Roles = "Admin,Root")]
    [HttpGet("create")]
    public IActionResult CreateUser()
    {
        return View();
    }
    
    [Authorize(Roles = "Admin,Root")]
    [HttpPost("create")]
    public async Task<IActionResult> CreateUser(CreateUserDto createUserDto)
    {
        if (!ModelState.IsValid)
            return View(createUserDto);

        var appCreateUserDto = new Application.Dtos.CreateUserDto
        {
            Username = createUserDto.Username,
            Email = createUserDto.Email,
            Phone = createUserDto.Phone,
            IsPublic = createUserDto.IsPublic,
            IsAdmin = createUserDto.IsAdmin,
            Password = createUserDto.Password
        };

        var result = await _usersService.CreateUser(HttpContext.User, appCreateUserDto);

        if (!result.IsSuccessful)
        {
            this.BadRequestView(result.Errors);
        }

        return RedirectToAction("GetUser", new {userName = createUserDto.Username});
    }

    [Authorize(Roles = "Admin,Root")]
    [HttpGet("{userId:guid}/update")]
    public async Task<IActionResult> UpdateUser(Guid userId)
    {
        var user = await _usersService.GetById(HttpContext.User, userId);
        if (user is null) return this.NotFoundView("User was not found");
        
        var updateUserDto = new UpdateUserDto
        {
            Username = user.User.UserName,
            IsAdmin = user.Roles.Contains(nameof(Role.Admin)),
            ClearAbout = false,
            SetPrivate = false
        };
        
        return View(updateUserDto);
    }
    
    [Authorize(Roles = "Admin,Root")]
    [HttpPost("{userId:guid}/update")]
    public async Task<IActionResult> UpdateUser(Guid userId, UpdateUserDto updateUserDto)
    {
        if (!ModelState.IsValid)
            return View(updateUserDto);
        
        var appUpdateUserDto = new Application.Dtos.UpdateUserDto
        {
            UserId = userId,
            Username = updateUserDto.Username,
            ClearAbout = updateUserDto.ClearAbout,
            IsAdmin = updateUserDto.IsAdmin,
            SetPrivate = updateUserDto.SetPrivate
        };
        
        var result = await _usersService.UpdateUser(HttpContext.User, appUpdateUserDto);
        
        if (!result.IsSuccessful)
        {
            this.BadRequestView(result.Errors);
        }

        return RedirectToAction("GetUser", new {userName = appUpdateUserDto.Username});
    }

    [Authorize(Roles = "Admin,Root")]
    [HttpGet("{userId:guid}/delete")]
    public async Task<IActionResult> DeleteUser([FromRoute] Guid userId)
    {
        var result = await _usersService.DeleteUser(HttpContext.User, userId);
        if (!result.IsSuccessful) return this.BadRequestView(result.Errors);
        return RedirectToAction("GetAllUsers");
    }
}